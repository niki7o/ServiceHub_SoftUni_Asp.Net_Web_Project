using ServiceHub.Data.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> All();
        IQueryable<T> AllAsNoTracking();

        Task<T?> GetByIdAsync(object id);

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveChangesAsync();
    }
}
