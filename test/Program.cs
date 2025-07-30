using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Дефинирайте тези класове временно, за да може PasswordHasher да работи
// Уверете се, че namespace-овете съвпадат с тези във вашия ServiceHub.Data.Models
namespace ServiceHub.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsBusiness { get; set; }
        public DateTime? BusinessExpiresOn { get; set; }
    }
}

namespace ServiceHub.Common.Enum
{
    public enum AccessType
    {
        Free = 0,
        Paid = 1,
        Subscription = 2
    }
}

// Край на временните класове

public class Program
{
    public static async Task Main(string[] args)
    {
        var passwordHasher = new PasswordHasher<ServiceHub.Data.Models.ApplicationUser>();

        Console.WriteLine("--- Generated Data for HasData() ---");

        // --- Roles ---
        var adminRoleId = Guid.NewGuid().ToString();
        var businessRoleId = Guid.NewGuid().ToString();
        var userRoleId = Guid.NewGuid().ToString();

        Console.WriteLine("\n// --- Roles ---");
        Console.WriteLine($"// Admin Role Id: {adminRoleId}");
        Console.WriteLine($"// BusinessUser Role Id: {businessRoleId}");
        Console.WriteLine($"// User Role Id: {userRoleId}");

        Console.WriteLine("\nbuilder.Entity<IdentityRole>().HasData(");
        Console.WriteLine($"    new IdentityRole {{ Id = \"{adminRoleId}\", Name = \"Admin\", NormalizedName = \"ADMIN\" }},");
        Console.WriteLine($"    new IdentityRole {{ Id = \"{businessRoleId}\", Name = \"BusinessUser\", NormalizedName = \"BUSINESSUSER\" }},");
        Console.WriteLine($"    new IdentityRole {{ Id = \"{userRoleId}\", Name = \"User\", NormalizedName = \"USER\" }}");
        Console.WriteLine(");");

        // --- Users ---
        var adminUserId = Guid.NewGuid().ToString();
        var businessUserId = Guid.NewGuid().ToString();
        var regularUserId = Guid.NewGuid().ToString();

        var adminUser = new ServiceHub.Data.Models.ApplicationUser { UserName = "adminuser", Email = "admin@servicehub.com", EmailConfirmed = true, NormalizedEmail = "ADMIN@SERVICEHUB.COM", NormalizedUserName = "ADMINUSER" };
        var businessUser = new ServiceHub.Data.Models.ApplicationUser { UserName = "businessuser", Email = "business@servicehub.com", EmailConfirmed = true, NormalizedEmail = "BUSINESS@SERVICEHUB.COM", NormalizedUserName = "BUSINESSUSER" };
        var regularUser = new ServiceHub.Data.Models.ApplicationUser { UserName = "regularuser", Email = "user@servicehub.com", EmailConfirmed = true, NormalizedEmail = "USER@SERVICEHUB.COM", NormalizedUserName = "REGULARUSER" };

        var adminPasswordHash = passwordHasher.HashPassword(adminUser, "Admin123");
        var businessPasswordHash = passwordHasher.HashPassword(businessUser, "Business123");
        var regularPasswordHash = passwordHasher.HashPassword(regularUser, "User123");

        // SecurityStamp и ConcurrencyStamp са важни за Identity. Генерираме нови GUID-ове за тях.
        adminUser.SecurityStamp = Guid.NewGuid().ToString();
        adminUser.ConcurrencyStamp = Guid.NewGuid().ToString();
        businessUser.SecurityStamp = Guid.NewGuid().ToString();
        businessUser.ConcurrencyStamp = Guid.NewGuid().ToString();
        regularUser.SecurityStamp = Guid.NewGuid().ToString();
        regularUser.ConcurrencyStamp = Guid.NewGuid().ToString();

        Console.WriteLine("\n// --- Users ---");
        Console.WriteLine($"// Admin User Id: {adminUserId}");
        Console.WriteLine($"// Business User Id: {businessUserId}");
        Console.WriteLine($"// Regular User Id: {regularUserId}");

        Console.WriteLine("\nbuilder.Entity<ApplicationUser>().HasData(");
        Console.WriteLine($"    new ApplicationUser {{ Id = \"{adminUserId}\", UserName = \"adminuser\", NormalizedUserName = \"ADMINUSER\", Email = \"admin@servicehub.com\", NormalizedEmail = \"ADMIN@SERVICEHUB.COM\", EmailConfirmed = true, PasswordHash = \"{adminPasswordHash}\", SecurityStamp = \"{adminUser.SecurityStamp}\", ConcurrencyStamp = \"{adminUser.ConcurrencyStamp}\" }},");
        Console.WriteLine($"    new ApplicationUser {{ Id = \"{businessUserId}\", UserName = \"businessuser\", NormalizedUserName = \"BUSINESSUSER\", Email = \"business@servicehub.com\", NormalizedEmail = \"BUSINESS@SERVICEHUB.COM\", EmailConfirmed = true, PasswordHash = \"{businessPasswordHash}\", SecurityStamp = \"{businessUser.SecurityStamp}\", ConcurrencyStamp = \"{businessUser.ConcurrencyStamp}\" }},");
        Console.WriteLine($"    new ApplicationUser {{ Id = \"{regularUserId}\", UserName = \"regularuser\", NormalizedUserName = \"REGULARUSER\", Email = \"user@servicehub.com\", NormalizedEmail = \"USER@SERVICEHUB.COM\", EmailConfirmed = true, PasswordHash = \"{regularPasswordHash}\", SecurityStamp = \"{regularUser.SecurityStamp}\", ConcurrencyStamp = \"{regularUser.ConcurrencyStamp}\" }}");
        Console.WriteLine(");");

        // --- User Roles ---
        Console.WriteLine("\n// --- User Roles ---");
        Console.WriteLine("\nbuilder.Entity<IdentityUserRole<string>>().HasData(");
        Console.WriteLine($"    new IdentityUserRole<string> {{ UserId = \"{adminUserId}\", RoleId = \"{adminRoleId}\" }},");
        Console.WriteLine($"    new IdentityUserRole<string> {{ UserId = \"{businessUserId}\", RoleId = \"{businessRoleId}\" }},");
        Console.WriteLine($"    new IdentityUserRole<string> {{ UserId = \"{regularUserId}\", RoleId = \"{userRoleId}\" }}");
        Console.WriteLine(");");

        Console.WriteLine("\n--- End Generated Data ---");
    }
}

