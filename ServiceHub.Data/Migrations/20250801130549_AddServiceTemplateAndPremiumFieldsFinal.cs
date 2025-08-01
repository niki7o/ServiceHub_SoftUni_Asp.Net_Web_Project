using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ServiceHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceTemplateAndPremiumFieldsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsBusiness = table.Column<bool>(type: "bit", nullable: false),
                    BusinessExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
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
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
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
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
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
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessType = table.Column<int>(type: "int", nullable: false),
                    ViewsCount = table.Column<int>(type: "int", nullable: false),
                    ServiceConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Favorites_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0c8b3e8e-c25e-44d7-84f9-2c7b5a1b3e4f", "fe74c9d2-fc5c-4058-b95f-5c40a2a3c3f4", "BusinessUser", "BUSINESSUSER" },
                    { "1d9c4f9f-a36a-4d6b-b5e0-3d8c6b2a5f7e", "9879ecec-d1b8-48aa-9895-665e8f1df7ec", "User", "USER" },
                    { "99049752-95b1-477d-944a-f34589d31b09", "ab30b278-1fe4-4058-bdf5-c97b0317bb49", "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "BusinessExpiresOn", "ConcurrencyStamp", "Email", "EmailConfirmed", "IsBusiness", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o", 0, null, "17f2c00d-127e-4a60-8b55-b7d857a45938", "admin@servicehub.com", true, false, false, null, "ADMIN@SERVICEHUB.COM", "ADMINUSER", "AQAAAAIAAYagAAAAEHDyY+bWGj5b4NCEQ22sdDwwgOXUGzd14Jna1PWwgUGuAT5uDIm3rppo3ro8FK2jdw==", null, false, "81b41e60-bbfd-426e-a451-b324afac9042", false, "adminuser" },
                    { "3f8b6c7d-e5f6-4g8h-i9j0-1k2l3m4n5o6p", 0, null, "bb81e0bc-25e2-4eda-9092-eaefb8045765", "business@servicehub.com", true, false, false, null, "BUSINESS@SERVICEHUB.COM", "BUSINESSUSER", "AQAAAAIAAYagAAAAEDvbXwCicbCkwIgkmtihHz+xB9VVltKmrmML+xT00yGnQH57wYtvDJ18a/xQQWvCXA==", null, false, "91973d3d-b866-422d-b759-6dc734d7fad2", false, "businessuser" },
                    { "4g9c7d8e-f6g7-4h9i-j0k1-2l3m4n5o6p7q", 0, null, "960af341-2093-42d2-9174-7a28273b0431", "user@servicehub.com", true, false, false, null, "USER@SERVICEHUB.COM", "REGULARUSER", "AQAAAAIAAYagAAAAEKY0c1iTAtyn5l0NSl/Trn0F1PZ9MRgXUKO2ErqWpvmLb0X7LhGC0RoeprNGZ2paXg==", null, false, "11d5fbe2-e03e-4300-869c-4c72a147b85b", false, "regularuser" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedOn", "Description", "ModifiedOn", "Name" },
                values: new object[,]
                {
                    { new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"), new DateTime(2025, 8, 1, 13, 5, 48, 82, DateTimeKind.Utc).AddTicks(3671), "Инструменти за работа с документи.", null, "Документи" },
                    { new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"), new DateTime(2025, 8, 1, 13, 5, 48, 82, DateTimeKind.Utc).AddTicks(3689), "Различни общи инструменти.", null, "Инструменти" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "99049752-95b1-477d-944a-f34589d31b09", "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o" },
                    { "0c8b3e8e-c25e-44d7-84f9-2c7b5a1b3e4f", "3f8b6c7d-e5f6-4g8h-i9j0-1k2l3m4n5o6p" },
                    { "1d9c4f9f-a36a-4d6b-b5e0-3d8c6b2a5f7e", "4g9c7d8e-f6g7-4h9i-j0k1-2l3m4n5o6p7q" }
                });

            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "AccessType", "CategoryId", "CreatedOn", "Description", "ModifiedOn", "ServiceConfigJson", "Title", "ViewsCount" },
                values: new object[,]
                {
                    { new Guid("1d4ae40b-c305-47b7-beed-163c4a0aeb40"), 1, new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(613), "Конвертира различни файлови формати (напр. PDF към DOCX, JPG към PNG).", null, "{\"toolName\": \"FileConverter\", \"endpoint\": \"/api/FileConverter/convert\", \"method\": \"POST\"}", "Конвертор на Файлове", 0 },
                    { new Guid("2ef43d87-d749-4d7d-9b7d-f7c4f527bea7"), 2, new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(640), "Изчислява ROI, бюджети, прогнозни приходи и разходи.", null, "{\"toolName\": \"FinancialCalculator\", \"endpoint\": \"/api/FinancialCalculator/calculate\", \"method\": \"POST\"}", "Финансов Калкулатор / Анализатор", 0 },
                    { new Guid("3a7b8b0c-1d2e-4f5a-a837-3d5e9f1a2b0c"), 0, new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(735), "Преброява думи, символи и редове във въведен текст.", null, "{\"toolName\": \"WordCharacterCounter\", \"endpoint\": \"/api/WordCharacter/count\", \"method\": \"POST\"}", "Брояч на Думи и Символи", 0 },
                    { new Guid("8edc2d04-00f5-4630-b5a9-4fa499fc7210"), 2, new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(739), "Генерира автоматично договори с шаблони (наем, труд и др.).", null, "{\"toolName\": \"ContractGenerator\", \"endpoint\": \"/api/ContractGenerator/generate\", \"method\": \"POST\"}", "Генератор на Договори", 0 },
                    { new Guid("b422f89b-e7a3-4130-b899-7b56010007e0"), 2, new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(636), "Въвеждаш данни и получаваш изчислена фактура.", null, "{\"toolName\": \"InvoiceGenerator\", \"endpoint\": \"/api/InvoiceGenerator/generate\", \"method\": \"POST\"}", "Генератор на Инвойси/Фактури", 0 },
                    { new Guid("c10de2fa-b49b-4c0d-9e8f-142b3cd40e6f"), 0, new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(624), "Преобразува текст в главни букви, малки букви или заглавен регистър.", null, "{\"toolName\": \"TextCaseConverter\", \"endpoint\": \"/api/TextCaseConverter/convert\", \"method\": \"POST\"}", "Конвертор на Текст (Главни/Малки букви)", 0 },
                    { new Guid("e11e539c-0290-4171-b606-16628d1790b0"), 1, new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(621), "Преобразува код между програмни езици (напр. C# към Python).", null, "{\"toolName\": \"CodeConverter\", \"endpoint\": \"/api/CodeConverter/convert\", \"method\": \"POST\"}", "Конвертор на Кодови Снипети", 0 },
                    { new Guid("f0c72c7b-709d-44b7-81c1-1e5ab73305ec"), 2, new Guid("a0a0a0a0-a0a0-a0a0-a0a0-000000000001"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(628), "Въвеждаш данни и получаваш готово CV в PDF формат.", null, "{\"toolName\": \"CVGenerator\", \"endpoint\": \"/api/CVGenerator/generate\", \"method\": \"POST\"}", "Автоматично CV/Резюме", 0 },
                    { new Guid("f5e402c0-91ba-4f8e-97d0-3b443fe10d3c"), 0, new Guid("b1b1b1b1-b1b1-b1b1-b1b1-000000000002"), new DateTime(2025, 8, 1, 13, 5, 48, 83, DateTimeKind.Utc).AddTicks(631), "Генерира силни, случайни пароли с конфигурируеми опции.", null, "{\"toolName\": \"PasswordGenerator\", \"endpoint\": \"/api/PasswordGenerator/generate\", \"method\": \"GET\"}", "Генератор на Случайни Пароли", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

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
                name: "IX_Favorites_ServiceId",
                table: "Favorites",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId",
                table: "Favorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ServiceId",
                table: "Reviews",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CategoryId",
                table: "Services",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
