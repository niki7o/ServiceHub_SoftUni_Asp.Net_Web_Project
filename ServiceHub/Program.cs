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
using System.Text.Json;
using System.Text.Json.Serialization;



namespace ServiceHub
{
    public class Program
    {
        public static async Task Main(string[] args)
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


builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // Prevents circular reference issues
        options.JsonSerializerOptions.WriteIndented = true; // For better readability in development
    });
builder.Services.AddRazorPages();

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// Register Services
builder.Services.AddScoped<IServiceService, ServicesService>();
builder.Services.AddScoped<IReviewService, ReviewsService>();
builder.Services.AddScoped<IFileConverterService, FileConverterService>();

// Register the new services
builder.Services.AddScoped<IWordCharacterCounterService, WordCharacterCounterService>();
builder.Services.AddScoped<ITextCaseConverterService, TextCaseConverterService>();
builder.Services.AddScoped<IRandomPasswordGeneratorService, RandomPasswordGeneratorService>();
 builder.Services.AddScoped<ICvGeneratorService, CvGeneratorService>();

            builder.Services.AddScoped<IServiceDispatcher, ServiceDispatcher>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ServiceDispatcher>>();
    var serviceImplementations = new Dictionary<Guid, Type>
                {
                    { ServiceConstants.FileConverterServiceId, typeof(IFileConverterService) },
                    { ServiceConstants.AutoCvResumeServiceId, typeof(ICvGeneratorService) }
                };
    return new ServiceDispatcher(sp, logger, serviceImplementations);
});

var app = builder.Build();

// IMPORTANT: Execute DataSeeder in a separate scope to ensure dependencies are resolved
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<ServiceHubDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply database migrations
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");

        // Execute DataSeeder methods
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

app.Run();
        }
    }
}

