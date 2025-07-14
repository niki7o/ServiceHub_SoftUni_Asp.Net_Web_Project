using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceHub.Common;
using ServiceHub.Data;
using ServiceHub.Data.DataSeeder;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using ServiceHub.Services.Services;
using ServiceHub.Services.Services.Repository;



namespace ServiceHub
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
          
         
            var connectionString = builder.Configuration.GetConnectionString("ServiceHubDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ServiceHubDbContextConnection' not found.");
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
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            builder.Services.AddScoped<IServiceService, ServicesService>();
            builder.Services.AddScoped<IReviewService, ReviewsService>();


            builder.Services.AddScoped<IServiceDispatcher, ServiceDispatcher>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ServiceDispatcher>>();
                var serviceImplementations = new Dictionary<Guid, Type>
    {
        { ServiceConstants.FileConverterServiceId, typeof(IFileConverterService) }
       
        // { ServiceConstants.AiGrammarStyleCheckerServiceId, typeof(IAiGrammarStyleCheckerService) },
        // { ServiceConstants.AiDocumentSummarizerServiceId, typeof(IAiDocumentSummarizerService) },
        // { ServiceConstants.FinancialCalculatorAnalyzerServiceId, typeof(IFinancialCalculatorAnalyzerService) },
        // { ServiceConstants.ContractGeneratorServiceId, typeof(IContractGeneratorService) },
        // { ServiceConstants.WebPolicyGeneratorServiceId, typeof(IWebPolicyGeneratorService) },
        // { ServiceConstants.InvoiceFactureGeneratorServiceId, typeof(IInvoiceFactureGeneratorService) },
        // { ServiceConstants.CvResumeGeneratorServiceId, typeof(ICvResumeGeneratorService) },
        // { ServiceConstants.CodeSnippetConverterServiceId, typeof(ICodeSnippetConverterService) },
        // { ServiceConstants.MarketingSloganGeneratorServiceId, typeof(IMarketingSloganGeneratorService) }
    };
             
                return new ServiceDispatcher(sp,logger, serviceImplementations); 
            });
            builder.Services.AddScoped<IFileConverterService, FileConverterService>();

            
            var app = builder.Build();
            DataSeeder.SeedServicesAsync(app.Services);
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

            app.Run();
        }
    }
}
