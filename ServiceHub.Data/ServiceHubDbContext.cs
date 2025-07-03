using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ServiceHub.Data
{
    public class ServiceHubDbContext : IdentityDbContext
    {
        public ServiceHubDbContext(DbContextOptions<ServiceHubDbContext> options)
            : base(options)
        {
        }
    }
}
