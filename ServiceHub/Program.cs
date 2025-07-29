using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using ServiceHub.Common;
using ServiceHub.Data;
using ServiceHub.Data.DataSeeder;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using ServiceHub.Services.Services;
using ServiceHub.Services.Services.Repository;
using System.ComponentModel;
using System.Text.Json;

using System.Text.Json.Serialization;

namespace ServiceHub
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            var connectionString = builder.Configuration.GetConnectionString("ServiceHubDbContextConnection") ??
                                   throw new InvalidOperationException("Connection string 'ServiceHubDbContextConnection' not found.");
            builder.Services.AddDbContext<ServiceHubDbContext>(options =>
                                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ServiceHubDbContext>();


            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });
            builder.Services.AddRazorPages();

            builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            builder.Services.AddScoped<IServiceService, ServicesService>();
            builder.Services.AddScoped<IReviewService, ReviewsService>();

            builder.Services.AddScoped<IFileConverterService, FileConverterService>();
            builder.Services.AddScoped<IContractGeneratorService, ContractGeneratorService>();
            builder.Services.AddScoped<ICvGeneratorService, CvGeneratorService>(); 
            builder.Services.AddScoped<IWordCharacterCounterService, WordCharacterCounterService>();
            builder.Services.AddScoped<IRandomPasswordGeneratorService, RandomPasswordGeneratorService>();
            builder.Services.AddScoped<ITextCaseConverterService, TextCaseConverterService>();
            builder.Services.AddScoped<IInvoiceGeneratorService, InvoiceGeneratorService>();
            builder.Services.AddScoped<IServiceDispatcher, ServiceDispatcher>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceDispatcher>>();
                var serviceImplementations = new Dictionary<Guid, Type>
                {
                    { ServiceConstants.FileConverterServiceId, typeof(IFileConverterService) },
                    { ServiceConstants.AutoCvResumeServiceId, typeof(ICvGeneratorService) },
                    { ServiceConstants.WordCharacterCounterServiceId, typeof(IWordCharacterCounterService) },
                    { ServiceConstants.TextCaseConverterServiceId, typeof(ITextCaseConverterService) },
                    { ServiceConstants.RandomPasswordGeneratorServiceId, typeof(IRandomPasswordGeneratorService) },
                    { ServiceConstants.ContractGeneratorServiceId, typeof(IContractGeneratorService) },
                    { ServiceConstants.InvoiceReceiptGeneratorId, typeof(IInvoiceGeneratorService) },
                    //{ ServiceConstants.CodeSnippetConverterServiceId, typeof(ICodeSnippetConverterService) }
                };
                return new ServiceDispatcher(sp, logger, serviceImplementations);
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var dbContext = services.GetRequiredService<ServiceHubDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    logger.LogInformation("Applying database migrations...");
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully.");

                    logger.LogInformation("Starting Data Seeder execution...");
                    await DataSeeder.SeedRolesAsync(roleManager, logger);
                    await DataSeeder.SeedUsersAsync(userManager, logger);
                    await DataSeeder.SeedCategoriesAsync(dbContext, logger);
                    await DataSeeder.SeedServicesAsync(dbContext, logger);
                    logger.LogInformation("Data Seeder execution completed.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during database migration or data seeding.");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.MapControllers();

            app.Run();
        }
    }
}