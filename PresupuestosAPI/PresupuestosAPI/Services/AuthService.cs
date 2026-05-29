using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PresupuestosAPI.Data;
using PresupuestosAPI.Models;
using PresupuestosAPI.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.DTOs.Auth;

namespace PresupuestosAPI.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly CurrentUserService _currentUserService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context,
            IOptions<JwtSettings> jwtOptions,
            CurrentUserService currentUserService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtSettings = jwtOptions.Value;
            _currentUserService = currentUserService;
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

        private static string HashToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);
            return Convert.ToBase64String(hashBytes);
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

            var freePlan = await _context.Plans
                .FirstOrDefaultAsync(p => p.Name == "Free" && p.IsActive);

            if (freePlan == null)
            {
                throw new InvalidOperationException("El plan Free no está configurado.");
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