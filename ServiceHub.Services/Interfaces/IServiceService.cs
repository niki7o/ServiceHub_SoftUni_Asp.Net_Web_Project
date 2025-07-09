using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IServiceService
    {

        Task<IEnumerable<ServiceViewModel>> GetAllAsync(string? filter = null, string? sort = null, string? currentUserId = null);
        Task<ServiceViewModel> GetByIdAsync(Guid id);
        Task CreateAsync(ServiceFormModel model);
        Task UpdateAsync(Guid id, ServiceFormModel model);
        Task DeleteAsync(Guid id);

        Task AddReviewAsync(Guid serviceId, string userId, ReviewFormModel model);


        Task<ServiceFormModel> GetServiceForEditAsync(Guid id);
    }
}
