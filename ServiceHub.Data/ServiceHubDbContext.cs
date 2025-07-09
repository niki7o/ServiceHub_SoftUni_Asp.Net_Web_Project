using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Data.Models;

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
                .HasForeignKey(r => r.ServiceId) 
                .OnDelete(DeleteBehavior.Cascade); 


            builder.Entity<Service>()
                .HasOne(s => s.Category)       
                .WithMany(c => c.Services)      
                .HasForeignKey(s => s.CategoryId) 
                .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}

