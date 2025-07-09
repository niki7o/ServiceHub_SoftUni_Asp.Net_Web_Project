using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public static async Task SeedServicesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ServiceHubDbContext>();
    
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

           
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                logger.LogInformation("Creating 'Admin' role...");
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

          
            var adminEmail = "admin@servicehub.com"; 
            var adminPassword = "Admin123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                logger.LogInformation($"Creating admin user '{adminEmail}'...");
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true 
                };
                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    logger.LogInformation($"Admin user '{adminEmail}' created successfully.");
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation($"Admin user '{adminEmail}' assigned to 'Admin' role.");
                    }
                    else
                    {
                        logger.LogError($"Failed to assign '{adminEmail}' to 'Admin' role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    logger.LogError($"Failed to create admin user '{adminEmail}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
            
                logger.LogInformation($"Assigning existing user '{adminEmail}' to 'Admin' role...");
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (addToRoleResult.Succeeded)
                {
                    logger.LogInformation($"User '{adminEmail}' assigned to 'Admin' role.");
                }
                else
                {
                    logger.LogError($"Failed to assign existing user '{adminEmail}' to 'Admin' role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                }
            }
            await dbContext.Database.MigrateAsync();

            if (!dbContext.Services.Any())
            {
                logger.LogInformation("Attempting to seed initial service data from embedded resource...");

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
                      
                        var serviceSeedModels = JsonSerializer.Deserialize<List<ServiceSeedModel>>(json);

                        if (serviceSeedModels != null && serviceSeedModels.Any())
                        {
                            var servicesToInsert = new List<Service>();

                            foreach (var seedModel in serviceSeedModels)
                            {
                             
                                var category = await dbContext.Categories
                                                              .FirstOrDefaultAsync(c => c.Name == seedModel.Category);

                                if (category == null)
                                {
                                    category = new Category { Id = Guid.NewGuid(), Name = seedModel.Category };
                                    await dbContext.Categories.AddAsync(category);
                                   
                                    await dbContext.SaveChangesAsync();
                                }

                             
                                if (!Enum.TryParse(seedModel.AccessType, true, out AccessType accessTypeEnum))
                                {
                                    logger.LogWarning($"Could not parse AccessType '{seedModel.AccessType}' for service '{seedModel.Title}'. Defaulting to 'Free'.");
                                    accessTypeEnum = AccessType.Free; 
                                }

                              
                                var service = new Service
                                {
                                    Id = Guid.NewGuid(), 
                                    Title = seedModel.Title,
                                    Description = seedModel.Description,
                                    IsBusinessOnly = false, 
                                    AccessType = accessTypeEnum,
                                    CategoryId = category.Id, 
                                    CreatedOn = DateTime.UtcNow 
                                };
                                servicesToInsert.Add(service);
                            }

                          
                            await dbContext.Services.AddRangeAsync(servicesToInsert);
                            await dbContext.SaveChangesAsync(); 

                            logger.LogInformation("Service data seeded successfully.");
                        }
                        else
                        {
                            logger.LogWarning("No service data found in embedded JSON resource, or deserialization failed. Nothing to seed.");
                        }
                    }
                }
            }
            else
            {
                logger.LogInformation("Service data already exists in the database. Skipping seeding.");
            }
        }
    }
}

