using Microsoft.EntityFrameworkCore;
using ServiceHub.Data;
using ServiceHub.Data.Models.Repository;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Repository
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

        public async Task<IEnumerable<T>> GetAllAsync() => await dbSet.ToListAsync();

        public async Task<T?> GetByIdAsync(Guid id) => await dbSet.FindAsync(id);

        public async Task AddAsync(T entity) => await dbSet.AddAsync(entity);

        public void Update(T entity) => dbSet.Update(entity);

        public void Delete(T entity) => dbSet.Remove(entity);

        public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
    }
}
