using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceHub.Common;
using ServiceHub.Common.Enum;
using ServiceHub.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceHub.Data.DataSeeder
{
 

        public static class DataSeeder
        {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger<object> logger)
        {
            logger.LogInformation("Starting role seeding...");

            string[] roleNames = { "Admin", "BusinessUser", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Role '{roleName}' created successfully.");
                    }
                    else
                    {
                        logger.LogError($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogInformation($"Role '{roleName}' already exists. Skipping creation.");
                }
            }
            logger.LogInformation("Role seeding completed.");
        }

        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger<object> logger)
        {
            logger.LogInformation("Starting user seeding...");

            string adminEmail = "admin@servicehub.com";
            string adminPassword = "Admin123";
            await CreateUserAndAssignRole(userManager, logger, adminEmail, adminPassword, "Admin");

            string businessEmail = "business@servicehub.com";
            string businessPassword = "Business123";
            await CreateUserAndAssignRole(userManager, logger, businessEmail, businessPassword, "BusinessUser");

            string userEmail = "user@servicehub.com";
            string userPassword = "User123";
            await CreateUserAndAssignRole(userManager, logger, userEmail, userPassword, "User");

            logger.LogInformation("User seeding completed.");
        }

        private static async Task CreateUserAndAssignRole(UserManager<ApplicationUser> userManager, ILogger<object> logger, string email, string password, string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                logger.LogInformation($"Creating user '{email}'...");
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    logger.LogInformation($"User '{email}' created successfully.");
                    var addToRoleResult = await userManager.AddToRoleAsync(user, roleName);
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation($"User '{email}' assigned to '{roleName}' role.");
                    }
                    else
                    {
                        logger.LogError($"Failed to assign '{email}' to '{roleName}' role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogError($"Failed to create user '{email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else if (!await userManager.IsInRoleAsync(user, roleName))
            {
                logger.LogInformation($"Assigning existing user '{email}' to '{roleName}' role...");
                var addToRoleResult = await userManager.AddToRoleAsync(user, roleName);
                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation($"User '{email}' assigned to '{roleName}' role.");
                }
                else
                {
                    logger.LogError($"Failed to assign existing user '{email}' to '{roleName}' role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"User '{email}' already exists and is in '{roleName}' role. Skipping creation/assignment.");
            }
        }

        public static async Task SeedCategoriesAsync(ServiceHubDbContext dbContext, ILogger<object> logger)
        {
            logger.LogInformation("Starting category seeding...");

            if (!await dbContext.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Документи" },
                    new Category { Name = "Инструменти" }
                };
                await dbContext.Categories.AddRangeAsync(categories);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Initial categories seeded successfully.");
            }
            else
            {
                logger.LogInformation("Categories already exist in the database. Skipping seeding.");
            }
            logger.LogInformation("Category seeding completed.");
        }

        public static async Task SeedServicesAsync(ServiceHubDbContext dbContext, ILogger<object> logger)
        {
            logger.LogInformation("Starting service data seeding from embedded JSON...");

           
            if (!await dbContext.Services.AnyAsync(s => s.Id == ServiceConstants.FileConverterServiceId))
            {
                var assembly = typeof(DataSeeder).Assembly;
                var resourceName = "ServiceHub.Data.DataSeeder.services.json";

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        
                        logger.LogError($"Embedded resource '{resourceName}' not found. Make sure 'services.json' is set to 'Build Action: Embedded Resource' in its properties, and its location matches the resourceName.");
                        throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
                    }
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var json = await reader.ReadToEndAsync();
                        logger.LogInformation($"Successfully read services.json content. Length: {json.Length}");
                        try
                        {
                            // Ensure ServiceSeedModel matches your JSON structure
                            var serviceSeedModels = JsonSerializer.Deserialize<List<ServiceSeedModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (serviceSeedModels != null && serviceSeedModels.Any())
                            {
                                logger.LogInformation($"Deserialized {serviceSeedModels.Count} services from JSON.");
                                var servicesToInsert = new List<Service>();
                                foreach (var seedModel in serviceSeedModels)
                                {
                                    if (await dbContext.Services.AnyAsync(s => s.Id == seedModel.Id))
                                    {
                                        logger.LogInformation($"Service '{seedModel.Title}' ({seedModel.Id}) already exists. Skipping.");
                                        continue;
                                    }

                                    var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == seedModel.Category);
                                    if (category == null)
                                    {
                                        logger.LogWarning($"Category '{seedModel.Category}' not found for service '{seedModel.Title}'. Creating new category.");
                                        category = new Category { Id = Guid.NewGuid(), Name = seedModel.Category };
                                        await dbContext.Categories.AddAsync(category);
                                        await dbContext.SaveChangesAsync();
                                    }

                                    if (!Enum.TryParse(seedModel.AccessType, true, out AccessType accessTypeEnum))
                                    {
                                        logger.LogWarning($"Could not parse AccessType '{seedModel.AccessType}' for service '{seedModel.Title}'. Defaulting to 'Free'.");
                                        accessTypeEnum = AccessType.Free; // Default to Free if parse fails
                                    }

                                    var service = new Service
                                    {
                                        Id = seedModel.Id,
                                        Title = seedModel.Title,
                                        Description = seedModel.Description,
                                        
                                        AccessType = accessTypeEnum,
                                        CategoryId = category.Id,
                                        CreatedOn = DateTime.UtcNow
                                    };
                                    servicesToInsert.Add(service);
                                }

                                if (servicesToInsert.Any())
                                {
                                    await dbContext.Services.AddRangeAsync(servicesToInsert);
                                    await dbContext.SaveChangesAsync();
                                    logger.LogInformation($"Successfully seeded {servicesToInsert.Count} new services.");
                                }
                                else
                                {
                                    logger.LogInformation("All services from JSON already exist or no new services to insert.");
                                }
                            }
                            else
                            {
                                logger.LogWarning("No service data found in embedded JSON resource, or deserialization failed. Nothing to seed.");
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            logger.LogError(jsonEx, "JSON deserialization failed for services.json. Check JSON format and ServiceSeedModel.");
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An unexpected error occurred during service seeding.");
                        }
                    }
                }
            }
            else
            {
                logger.LogInformation("Service data already exists in the database. Skipping seeding.");
            }
            logger.LogInformation("Service data seeding completed.");
        }
    }
}

