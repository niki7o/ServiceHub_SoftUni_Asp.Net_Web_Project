using ServiceHub.Core.Models;
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

        Task<ServiceAllViewModel> GetAllAsync(string? categoryFilter = null, string? accessTypeFilter = null, string? filter = null, string? sort = null, string? currentUserId = null, int currentPage = 1, int servicesPerPage = 9);
        Task<ServiceViewModel> GetByIdAsync(Guid id, string? currentUserId = null, int reviewPage = 1, int reviewsPerPage = 2);
        Task CreateAsync(ServiceFormModel model, string userId);
        Task UpdateAsync(Guid id, ServiceFormModel model, string editorId, bool isAdmin);
        Task DeleteAsync(Guid id, string deleterId, bool isAdmin);

      

        Task IncrementViewsCount(Guid serviceId);

        Task<ServiceFormModel> GetServiceForEditAsync(Guid id, string editorId, bool isAdmin);

        Task ToggleFavorite(Guid serviceId, string userId);

        Task AddServiceTemplateAsync(ServiceFormModel model, string userId, bool isAdmin);
        Task<IEnumerable<ServiceViewModel>> GetAllPendingTemplatesAsync();
        Task ApproveServiceTemplateAsync(Guid serviceId, string adminId);
        Task RejectServiceTemplateAsync(Guid serviceId, string adminId);

        Task<IEnumerable<ServiceViewModel>> GetCreatedServicesByUserIdAsync(string userId);
        Task<IEnumerable<ServiceViewModel>> GetFavoriteServicesByUserIdAsync(string userId);
        Task<IEnumerable<ReviewViewModel>> GetReviewsByUserIdAsync(string userId);
        Task<int> GetApprovedServicesCountByUserIdAsync(string userId);
        Task<IEnumerable<ServiceViewModel>> SearchServicesByTitleAsync(string searchTerm);
    }

   
}
