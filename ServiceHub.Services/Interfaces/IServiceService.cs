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

        Task<IEnumerable<ServiceViewModel>> GetAllAsync(string? categoryFilter = null, string? accessTypeFilter = null, string? filter = null, string? sort = null, string? currentUserId = null);
        Task<ServiceViewModel> GetByIdAsync(Guid id, string? currentUserId);

        Task CreateAsync(ServiceFormModel model, string userId);

        Task UpdateAsync(Guid id, ServiceFormModel model, string editorId, bool isAdmin);

        Task DeleteAsync(Guid id, string deleterId, bool isAdmin);

        Task AddReviewAsync(Guid serviceId, string userId, ReviewFormModel model);

        Task IncrementViewsCount(Guid serviceId);

        Task<ServiceFormModel> GetServiceForEditAsync(Guid id, string editorId, bool isAdmin);

        Task ToggleFavorite(Guid serviceId, string userId);

        Task AddServiceTemplateAsync(ServiceFormModel model, string userId, bool isAdmin);
        Task<IEnumerable<ServiceViewModel>> GetAllPendingTemplatesAsync();
        Task ApproveServiceTemplateAsync(Guid serviceId, string adminId);
        Task RejectServiceTemplateAsync(Guid serviceId, string adminId);
    }
}
