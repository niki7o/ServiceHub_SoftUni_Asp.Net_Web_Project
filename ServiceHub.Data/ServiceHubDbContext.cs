using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Common.Enum;

using ServiceHub.Data.Models;
using System.Text.Json;

namespace ServiceHub.Data
{
    public class ServiceHubDbContext : IdentityDbContext<ApplicationUser>
    {
        public ServiceHubDbContext(DbContextOptions<ServiceHubDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Favorite>()
                .HasOne(f => f.Service)
                .WithMany(s => s.Favorites)
                .HasForeignKey(f => f.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Service)
                .WithMany(s => s.Reviews)
                .HasForeignKey(s => s.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Service>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            
            var categoryDocumentsId = Guid.Parse("A0A0A0A0-A0A0-A0A0-A0A0-000000000001");
            var categoryToolsId = Guid.Parse("B1B1B1B1-B1B1-B1B1-B1B1-000000000002");

            builder.Entity<Category>().HasData(
                new Category { Id = categoryDocumentsId, Name = "Документи", Description = "Инструменти за работа с документи.", CreatedOn = DateTime.UtcNow },
                new Category { Id = categoryToolsId, Name = "Инструменти", Description = "Различни общи инструменти.", CreatedOn = DateTime.UtcNow }
            );

         
            const string ServicesJsonContent = @"[
                {
                    ""Id"": ""1D4AE40B-C305-47B7-BEED-163C4A0AEB40"",
                    ""Title"": ""Конвертор на Файлове"",
                    ""Description"": ""Конвертира различни файлови формати (напр. PDF към DOCX, JPG към PNG)."",
                    ""Category"": ""Документи"",
                    ""AccessType"": ""Partial"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""FileConverter\"", \""endpoint\"": \""/api/FileConverter/convert\"", \""method\"": \""POST\""}""
                },
                {
                    ""Id"": ""E11E539C-0290-4171-B606-16628D1790B0"",
                    ""Title"": ""Конвертор на Кодови Снипети"",
                    ""Description"": ""Преобразува код между програмни езици (напр. C# към Python)."",
                    ""Category"": ""Инструменти"",
                    ""AccessType"": ""Partial"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""CodeConverter\"", \""endpoint\"": \""/api/CodeConverter/convert\"", \""method\"": \""POST\""}""
                },
                {
                    ""Id"": ""C10DE2FA-B49B-4C0D-9E8F-142B3CD40E6F"",
                    ""Title"": ""Конвертор на Текст (Главни/Малки букви)"",
                    ""Description"": ""Преобразува текст в главни букви, малки букви или заглавен регистър."",
                    ""Category"": ""Инструменти"",
                    ""AccessType"": ""Free"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""TextCaseConverter\"", \""endpoint\"": \""/api/TextCaseConverter/convert\"", \""method\"": \""POST\""}""
                },
                {
                    ""Id"": ""F0C72C7B-709D-44B7-81C1-1E5AB73305EC"",
                    ""Title"": ""Автоматично CV/Резюме"",
                    ""Description"": ""Въвеждаш данни и получаваш готово CV в PDF формат."",
                    ""Category"": ""Документи"",
                    ""AccessType"": ""Premium"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""CVGenerator\"", \""endpoint\"": \""/api/CVGenerator/generate\"", \""method\"": \""POST\""}""
                },
                {
                    ""Id"": ""F5E402C0-91BA-4F8E-97D0-3B443FE10D3C"",
                    ""Title"": ""Генератор на Случайни Пароли"",
                    ""Description"": ""Генерира силни, случайни пароли с конфигурируеми опции."",
                    ""Category"": ""Инструменти"",
                    ""AccessType"": ""Free"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""PasswordGenerator\"", \""endpoint\"": \""/api/PasswordGenerator/generate\"", \""method\"": \""GET\""}""
                },
                {
                    ""Id"": ""B422F89B-E7A3-4130-B899-7B56010007E0"",
                    ""Title"": ""Генератор на Инвойси/Фактури"",
                    ""Description"": ""Въвеждаш данни и получаваш изчислена фактура."",
                    ""Category"": ""Документи"",
                    ""AccessType"": ""Premium"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""InvoiceGenerator\"", \""endpoint\"": \""/api/InvoiceGenerator/generate\"", \""method\"": \""POST\""}""
                },
                {
                    ""Id"": ""2EF43D87-D749-4D7D-9B7D-F7C4F527BEA7"",
                    ""Title"": ""Финансов Калкулатор / Анализатор"",
                    ""Description"": ""Изчислява ROI, бюджети, прогнозни приходи и разходи."",
                    ""Category"": ""Инструменти"",
                    ""AccessType"": ""Premium"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""FinancialCalculator\"", \""endpoint\"": \""/api/FinancialCalculator/calculate\"", \""method\"": \""POST\""}""
                },
                {
                    ""Id"": ""3A7B8B0C-1D2E-4F5A-A837-3D5E9F1A2B0C"",
                    ""Title"": ""Брояч на Думи и Символи"",
                    ""Description"": ""Преброява думи, символи и редове във въведен текст."",
                    ""Category"": ""Инструменти"",
                    ""AccessType"": ""Free"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""WordCharacterCounter\"", \""endpoint\"": \""/api/WordCharacter/count\"", \""method\"": \""POST\""}""
                },
                {
                    ""Id"": ""8EDC2D04-00F5-4630-B5A9-4FA499FC7210"",
                    ""Title"": ""Генератор на Договори"",
                    ""Description"": ""Генерира автоматично договори с шаблони (наем, труд и др.)."",
                    ""Category"": ""Документи"",
                    ""AccessType"": ""Premium"",
                    ""ServiceConfigJson"": ""{\""toolName\"": \""ContractGenerator\"", \""endpoint\"": \""/api/ContractGenerator/generate\"", \""method\"": \""POST\""}""
                }
            ]";

           
            try
            {
                var serviceSeedModels = JsonSerializer.Deserialize<List<ServiceSeedModel>>(
                    ServicesJsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (serviceSeedModels != null)
                {
                    var servicesToSeed = new List<Service>();
                    foreach (var seedModel in serviceSeedModels)
                    {
                        Guid categoryId;
                        if (seedModel.Category == "Документи")
                        {
                            categoryId = categoryDocumentsId;
                        }
                        else if (seedModel.Category == "Инструменти")
                        {
                            categoryId = categoryToolsId;
                        }
                        else
{
    throw new InvalidOperationException($"Category '{seedModel.Category}' not found for service '{seedModel.Title}' during HasData seeding.");
}

AccessType accessTypeEnum;
if (!Enum.TryParse(seedModel.AccessType, true, out accessTypeEnum))
{
    accessTypeEnum = AccessType.Free;
}

servicesToSeed.Add(new Service
{
    Id = seedModel.Id,
    Title = seedModel.Title,
    Description = seedModel.Description,
    CategoryId = categoryId,
    AccessType = accessTypeEnum,
    ViewsCount = 0,
    ServiceConfigJson = seedModel.ServiceConfigJson,
    CreatedOn = DateTime.UtcNow
});
                    }
                    builder.Entity<Service>().HasData(servicesToSeed);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding services in OnModelCreating: {ex.Message}");
            }

           
            var adminRoleId = "99049752-95b1-477d-944a-f34589d31b09";
var businessRoleId = "0c8b3e8e-c25e-44d7-84f9-2c7b5a1b3e4f";
var userRoleId = "1d9c4f9f-a36a-4d6b-b5e0-3d8c6b2a5f7e";

builder.Entity<IdentityRole>().HasData(
    new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() },
    new IdentityRole { Id = businessRoleId, Name = "BusinessUser", NormalizedName = "BUSINESSUSER", ConcurrencyStamp = Guid.NewGuid().ToString() },
    new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() }
);


var adminUserId = "2e7a5b6c-d4e5-4f7g-h8i9-0j1k2l3m4n5o";
var businessUserId = "3f8b6c7d-e5f6-4g8h-i9j0-1k2l3m4n5o6p";
var regularUserId = "4g9c7d8e-f6g7-4h9i-j0k1-2l3m4n5o6p7q";

builder.Entity<ApplicationUser>().HasData(
    new ApplicationUser
    {
        Id = adminUserId,
        UserName = "adminuser",
        NormalizedUserName = "ADMINUSER",
        Email = "admin@servicehub.com",
        NormalizedEmail = "ADMIN@SERVICEHUB.COM",
        EmailConfirmed = true,
        PasswordHash = "AQAAAAIAAYagAAAAEHDyY+bWGj5b4NCEQ22sdDwwgOXUGzd14Jna1PWwgUGuAT5uDIm3rppo3ro8FK2jdw==", 
        SecurityStamp = Guid.NewGuid().ToString(),
        ConcurrencyStamp = Guid.NewGuid().ToString()
    },
    new ApplicationUser
    {
        Id = businessUserId,
        UserName = "businessuser",
        NormalizedUserName = "BUSINESSUSER",
        Email = "business@servicehub.com",
        NormalizedEmail = "BUSINESS@SERVICEHUB.COM",
        EmailConfirmed = true,
        PasswordHash = "AQAAAAIAAYagAAAAEDvbXwCicbCkwIgkmtihHz+xB9VVltKmrmML+xT00yGnQH57wYtvDJ18a/xQQWvCXA==", 
        SecurityStamp = Guid.NewGuid().ToString(),
        ConcurrencyStamp = Guid.NewGuid().ToString()
    },
    new ApplicationUser
    {
        Id = regularUserId,
        UserName = "regularuser",
        NormalizedUserName = "REGULARUSER",
        Email = "user@servicehub.com",
        NormalizedEmail = "USER@SERVICEHUB.COM",
        EmailConfirmed = true,
        PasswordHash = "AQAAAAIAAYagAAAAEKY0c1iTAtyn5l0NSl/Trn0F1PZ9MRgXUKO2ErqWpvmLb0X7LhGC0RoeprNGZ2paXg==", 
        SecurityStamp = Guid.NewGuid().ToString(),
        ConcurrencyStamp = Guid.NewGuid().ToString()
    }
);


builder.Entity<IdentityUserRole<string>>().HasData(
    new IdentityUserRole<string> { UserId = adminUserId, RoleId = adminRoleId },
    new IdentityUserRole<string> { UserId = businessUserId, RoleId = businessRoleId },
    new IdentityUserRole<string> { UserId = regularUserId, RoleId = userRoleId }
);
        }
    }
}

