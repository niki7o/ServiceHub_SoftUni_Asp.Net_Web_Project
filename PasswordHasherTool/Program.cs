using Microsoft.AspNetCore.Identity;
using System;

namespace PasswordHasherTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var passwordHasher = new PasswordHasher<IdentityUser>();

            // Генериране на хеш за "Admin123"
            var adminPassword = "Admin123";
            var adminHashedPassword = passwordHasher.HashPassword(new IdentityUser(), adminPassword);
            Console.WriteLine($"Admin Password: {adminPassword}");
            Console.WriteLine($"Admin Hash: {adminHashedPassword}");
            Console.WriteLine();

            // Генериране на хеш за "Business123"
            var businessPassword = "Business123";
            var businessHashedPassword = passwordHasher.HashPassword(new IdentityUser(), businessPassword);
            Console.WriteLine($"Business Password: {businessPassword}");
            Console.WriteLine($"Business Hash: {businessHashedPassword}");
            Console.WriteLine();

            // Генериране на хеш за "User123"
            var userPassword = "User123";
            var userHashedPassword = passwordHasher.HashPassword(new IdentityUser(), userPassword);
            Console.WriteLine($"User Password: {userPassword}");
            Console.WriteLine($"User Hash: {userHashedPassword}");
            Console.WriteLine();

            Console.WriteLine("Натиснете произволен клавиш, за да излезете...");
            Console.ReadKey();
        }
    }
}
