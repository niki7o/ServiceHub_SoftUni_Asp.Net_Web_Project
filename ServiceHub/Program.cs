using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using ServiceHub.Common;
using ServiceHub.Data;

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

           
            builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

          
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

           
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
            builder.Services.AddScoped<IFinancialCalculatorService, FinancialCalculatorService>();
            builder.Services.AddScoped<ICodeSnippetConverterService, CodeSnippetConverterService>();

           
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
                    { ServiceConstants.FinancialCalculatorAnalyzerId, typeof(IFinancialCalculatorService) },
                    { ServiceConstants.CodeSnippetConverterServiceId, typeof(ICodeSnippetConverterService) }
                };
                return new ServiceDispatcher(sp, logger, serviceImplementations);
            });

            var app = builder.Build();


            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<ServiceHubDbContext>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                    await dbContext.Database.MigrateAsync();

                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "[Startup] An error occurred while seeding the database. Application might not function correctly.");
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
                name: "Admin",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.MapControllers();

            app.Run(); 
        }

    }
}