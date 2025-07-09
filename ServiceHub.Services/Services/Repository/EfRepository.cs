using Microsoft.EntityFrameworkCore;
using ServiceHub.Data;
using ServiceHub.Data.Models;
using ServiceHub.Data.Models.Repository;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services.Repository
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {


         private readonly ServiceHubDbContext context; 
        private readonly DbSet<T> dbSet;

        public EfRepository(ServiceHubDbContext context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        public IQueryable<T> All()
        {
            return dbSet;
        }

        public IQueryable<T> AllAsNoTracking()
        {
            return dbSet.AsNoTracking();
        }

        public async Task AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            dbSet.Remove(entity);
        }

        public Task<int> SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }

        public async Task<T?> GetByIdAsync(object id)
        {
           

            if (typeof(T).IsSubclassOf(typeof(Microsoft.AspNetCore.Identity.IdentityUser)) || typeof(T) == typeof(ApplicationUser))
            {
               
                if (id is string stringId)
                {
                    return await dbSet.FindAsync(stringId);
                }
                else
                {
                    throw new ArgumentException($"Invalid ID type for {typeof(T).Name}. Expected string ID for IdentityUser based entities.");
                }
            }
            else if (typeof(T).IsSubclassOf(typeof(BaseEntity)) || typeof(T) == typeof(BaseEntity))
            {
              
                if (id is Guid guidId)
                {
                    return await dbSet.FindAsync(guidId);
                }
                else
                {
                   
                    if (id is string stringGuid && Guid.TryParse(stringGuid, out Guid parsedGuid))
                    {
                        return await dbSet.FindAsync(parsedGuid);
                    }
                    throw new ArgumentException($"Invalid ID type for {typeof(T).Name}. Expected Guid ID for BaseEntity based entities.");
                }
            }
            else
            {
              
                return await dbSet.FindAsync(id);
            }
        }
    }

}

