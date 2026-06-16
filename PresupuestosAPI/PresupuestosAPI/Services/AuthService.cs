using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PresupuestosAPI.Data;
using PresupuestosAPI.DTOs.Auth;
using PresupuestosAPI.Models;
using PresupuestosAPI.Services.Email;
using PresupuestosAPI.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PresupuestosAPI.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly CurrentUserService _currentUserService;
        private readonly GoogleAuthSettings _googleAuthSettings;
        private readonly FrontendSettings _frontendSettings;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context,
            IOptions<JwtSettings> jwtOptions,
            IOptions<GoogleAuthSettings> googleAuthOptions,
            IOptions<FrontendSettings> frontendOptions,
            CurrentUserService currentUserService,
            IEmailSender emailSender,
            ILogger<AuthService> logger
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtSettings = jwtOptions.Value;
            _googleAuthSettings = googleAuthOptions.Value;
            _currentUserService = currentUserService;
            _frontendSettings = frontendOptions.Value;
            _emailSender = emailSender;
            _logger = logger;
        }

        private string GenerateAccessToken(ApplicationUser user, int workspaceId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("workspaceId", workspaceId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private DateTime GetAccessTokenExpiration()
        {
            return DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
        }

        private static string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        private async Task<(Workspace Workspace, Plan Plan)> CreateInitialSaasSetupAsync(ApplicationUser user, string email)
        {
            var freePlan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Name == "Free" && p.IsActive);

            if (freePlan == null)
            {
                throw new InvalidOperationException("El plan Free no está configurado.");
            }

            var workspace = new Workspace
            {
                Name = $"Workspace de {email}",
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();

            var subscription = new Subscription
            {
                WorkspaceId = workspace.IdWorkspace,
                PlanId = freePlan.IdPlan,
                Status = "Active",
                StartDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var usage = new WorkspaceUsage
            {
                WorkspaceId = workspace.IdWorkspace,
                PdfExportsUsed = 0,
                PdfExportsPeriodStart = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            _context.WorkspaceUsages.Add(usage);

            await _context.SaveChangesAsync();

            return (workspace, freePlan);
        }

        private static string HashToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private static string EncodeToken(string token)
        {
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }

        private static string DecodeToken(string encodedToken)
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(encodedToken);

            return Encoding.UTF8.GetString(decodedBytes);
        }

        private static string BuildPasswordResetEmailBody(string resetPasswordUrl)
        {
            var safeResetPasswordUrl = WebUtility.HtmlEncode(resetPasswordUrl);

            return $@"
            <div style=""font-family: Arial, sans-serif; color: #111827; line-height: 1.5;"">
                <h2>Recuperá tu contraseña</h2>

                <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>MT Presupuestos</strong>.</p>

                <p>Para crear una nueva contraseña, hacé click en el siguiente botón:</p>

                <p>
                    <a href=""{safeResetPasswordUrl}""
                       style=""display:inline-block; padding:12px 18px; background:#111827; color:#ffffff; text-decoration:none; border-radius:8px;"">
                        Restablecer contraseña
                    </a>
                </p>

                <p>Si no solicitaste este cambio, podés ignorar este email.</p>

                <p style=""font-size: 13px; color: #6b7280;"">
                    MT Presupuestos - Presupuestos profesionales en minutos.
                </p>
            </div>";
        }

        private async Task<string> CreateRefreshTokenAsync(ApplicationUser user)
        {
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
            {
                throw new InvalidOperationException("Las contraseñas no coinciden.");
            }

            var email = dto.Email.Trim().ToLower();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Ya existe un usuario registrado con este email.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            var (workspace, freePlan) = await CreateInitialSaasSetupAsync(user, email);

            var accessToken = GenerateAccessToken(user, workspace.IdWorkspace);
            var refreshToken = await CreateRefreshTokenAsync(user);

            await transaction.CommitAsync();

            return new AuthResultDto
            {
                Response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = GetAccessTokenExpiration(),
                    Email = email,
                    WorkspaceId = workspace.IdWorkspace,
                    PlanName = freePlan.Name
                },
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResultDto> GoogleLoginAsync(GoogleLoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(_googleAuthSettings.ClientId))
            {
                throw new InvalidOperationException("Google ClientId no está configurado.");
            }

            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleAuthSettings.ClientId }
            };

            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, validationSettings);
            }
            catch
            {
                throw new UnauthorizedAccessException("Token de Google inválido.");
            }

            if (!payload.EmailVerified)
            {
                throw new UnauthorizedAccessException("El email de Google no está verificado.");
            }

            var email = payload.Email.Trim().ToLower();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var user = await _userManager.FindByLoginAsync("Google", payload.Subject);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(email);
            }

            Workspace workspace;
            Plan plan;

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(errors);
                }

                var loginInfo = new UserLoginInfo("Google", payload.Subject, "Google");
                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

                if (!addLoginResult.Succeeded)
                {
                    var errors = string.Join(" | ", addLoginResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(errors);
                }

                var setup = await CreateInitialSaasSetupAsync(user, email);
                workspace = setup.Workspace;
                plan = setup.Plan;
            }
            else
            {
                var existingGoogleLogin = await _userManager.FindByLoginAsync("Google", payload.Subject);

                if (existingGoogleLogin == null)
                {
                    var loginInfo = new UserLoginInfo("Google", payload.Subject, "Google");
                    var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);

                    if (!addLoginResult.Succeeded)
                    {
                        var errors = string.Join(" | ", addLoginResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException(errors);
                    }
                }

                workspace = await _context.Workspaces
                    .FirstOrDefaultAsync(w => w.UserId == user.Id)
                    ?? throw new InvalidOperationException("El usuario no tiene un workspace asociado.");

                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.WorkspaceId == workspace.IdWorkspace && s.Status == "Active");

                if (subscription == null || subscription.Plan == null)
                {
                    throw new InvalidOperationException("El usuario no tiene una suscripción activa.");
                }

                plan = subscription.Plan;
            }

            var accessToken = GenerateAccessToken(user, workspace.IdWorkspace);
            var refreshToken = await CreateRefreshTokenAsync(user);

            await transaction.CommitAsync();

            return new AuthResultDto
            {
                Response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = GetAccessTokenExpiration(),
                    Email = user.Email ?? email,
                    WorkspaceId = workspace.IdWorkspace,
                    PlanName = plan.Name
                },
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Email o contraseña incorrectos.");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync( 
                user,
                dto.Password,
                lockoutOnFailure: true 
            );

            if (!signInResult.Succeeded)
            {
                throw new UnauthorizedAccessException("Email o contraseña incorrectos.");
            }

            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (workspace == null)
            {
                throw new InvalidOperationException("El usuario no tiene un workspace asociado.");
            }

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.WorkspaceId == workspace.IdWorkspace && s.Status == "Active");

            if (subscription == null || subscription.Plan == null)
            {
                throw new InvalidOperationException("El usuario no tiene una suscripción activa.");
            }

            var accessToken = GenerateAccessToken(user, workspace.IdWorkspace);
            var refreshToken = await CreateRefreshTokenAsync(user);

            return new AuthResultDto
            {
                Response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = GetAccessTokenExpiration(),
                    Email = user.Email ?? string.Empty,
                    WorkspaceId = workspace.IdWorkspace,
                    PlanName = subscription.Plan.Name
                },
                RefreshToken = refreshToken
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return;
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (!hasPassword)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_frontendSettings.BaseUrl))
            {
                _logger.LogError("FrontendSettings:BaseUrl no está configurado.");
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = EncodeToken(token);

            var resetPasswordUrl = QueryHelpers.AddQueryString(
                $"{_frontendSettings.BaseUrl.TrimEnd('/')}/reset-password",
                new Dictionary<string, string?>
                {
                    ["userId"] = user.Id,
                    ["token"] = encodedToken
                }
            );

            var subject = "Recuperá tu contraseña - MT Presupuestos";
            var htmlBody = BuildPasswordResetEmailBody(resetPasswordUrl);

            try
            {
                await _emailSender.SendEmailAsync(email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de recuperación de contraseña.");
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                throw new InvalidOperationException("Las contraseñas no coinciden.");
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
            {
                throw new InvalidOperationException("No pudimos restablecer la contraseña. El link es inválido o expiró.");
            }

            string decodedToken;

            try
            {
                decodedToken = DecodeToken(dto.Token);
            }
            catch
            {
                throw new InvalidOperationException("No pudimos restablecer la contraseña. El link es inválido o expiró.");
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                dto.NewPassword
            );

            if (!result.Succeeded)
            {
                var hasInvalidToken = result.Errors.Any(e =>
                    e.Code.Contains("InvalidToken", StringComparison.OrdinalIgnoreCase));

                if (hasInvalidToken)
                {
                    throw new InvalidOperationException("No pudimos restablecer la contraseña. El link es inválido o expiró.");
                }

                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }
        }

        public async Task<CurrentUserResponseDto> GetCurrentUserAsync()
        {
            var userId = _currentUserService.GetUserId();
            var workspaceId = _currentUserService.GetWorkspaceId();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Usuario no autenticado.");
            }

            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.IdWorkspace == workspaceId && w.UserId == user.Id);

            if (workspace == null)
            {
                throw new InvalidOperationException("Workspace no encontrado para el usuario.");
            }

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.WorkspaceId == workspace.IdWorkspace && s.Status == "Active");

            if (subscription == null || subscription.Plan == null)
            {
                throw new InvalidOperationException("Suscripción activa no encontrada.");
            }

            return new CurrentUserResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                WorkspaceId = workspace.IdWorkspace,
                WorkspaceName = workspace.Name,
                PlanName = subscription.Plan.Name
            };
        }

        public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new UnauthorizedAccessException("Refresh token inválido.");
            }

            var tokenHash = HashToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == tokenHash &&
                    !rt.IsRevoked &&
                    rt.ExpiresAt > DateTime.UtcNow);

            if (storedToken == null || storedToken.User == null)
            {
                throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
            }

            var user = storedToken.User;

            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (workspace == null)
            {
                throw new InvalidOperationException("El usuario no tiene un workspace asociado.");
            }

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.WorkspaceId == workspace.IdWorkspace && s.Status == "Active");

            if (subscription == null || subscription.Plan == null)
            {
                throw new InvalidOperationException("El usuario no tiene una suscripción activa.");
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            var newAccessToken = GenerateAccessToken(user, workspace.IdWorkspace);
            var newRefreshToken = await CreateRefreshTokenAsync(user);

            return new AuthResultDto
            {
                Response = new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    AccessTokenExpiresAt = GetAccessTokenExpiration(),
                    Email = user.Email ?? string.Empty,
                    WorkspaceId = workspace.IdWorkspace,
                    PlanName = subscription.Plan.Name
                },
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var tokenHash = HashToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == tokenHash &&
                    !rt.IsRevoked);

            if (storedToken == null)
            {
                return;
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}