using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresupuestosAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuthSaas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    IdPlan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxCompanies = table.Column<int>(type: "int", nullable: false),
                    MaxPresupuestos = table.Column<int>(type: "int", nullable: false),
                    MaxPdfExports = table.Column<int>(type: "int", nullable: false),
                    PdfExportLimitPeriod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.IdPlan);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    IdRefreshToken = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.IdRefreshToken);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    IdWorkspace = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.IdWorkspace);
                    table.ForeignKey(
                        name: "FK_Workspaces_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    IdCompany = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorMain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorSecondary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.IdCompany);
                    table.ForeignKey(
                        name: "FK_Companies_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "IdWorkspace",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    IdSubscription = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MercadoPagoSubscriptionId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.IdSubscription);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "IdPlan",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "IdWorkspace",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceUsages",
                columns: table => new
                {
                    IdWorkspaceUsage = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    PdfExportsUsed = table.Column<int>(type: "int", nullable: false),
                    PdfExportsPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PdfExportsPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceUsages", x => x.IdWorkspaceUsage);
                    table.ForeignKey(
                        name: "FK_WorkspaceUsages_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "IdWorkspace",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    IdPresupuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BudgetNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaPresupuesto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IdCompany = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.IdPresupuesto);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Companies_IdCompany",
                        column: x => x.IdCompany,
                        principalTable: "Companies",
                        principalColumn: "IdCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PresupuestoItems",
                columns: table => new
                {
                    IdItem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Materials = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Labor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IdPresupuesto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestoItems", x => x.IdItem);
                    table.ForeignKey(
                        name: "FK_PresupuestoItems_Presupuestos_IdPresupuesto",
                        column: x => x.IdPresupuesto,
                        principalTable: "Presupuestos",
                        principalColumn: "IdPresupuesto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_WorkspaceId",
                table: "Companies",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestoItems_IdPresupuesto",
                table: "PresupuestoItems",
                column: "IdPresupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdCompany",
                table: "Presupuestos",
                column: "IdCompany");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_WorkspaceId",
                table: "Subscriptions",
                column: "WorkspaceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_UserId",
                table: "Workspaces",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceUsages_WorkspaceId",
                table: "WorkspaceUsages",
                column: "WorkspaceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "PresupuestoItems");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "WorkspaceUsages");

            migrationBuilder.DropTable(
                name: "Presupuestos");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
