using ServiceHub.Core.Models.Reviews;
using ServiceHub.Data.Models;

namespace ServiceHub.Services.Interfaces
{
    public interface IReviewService
    {
        Task AddReviewAsync(Guid serviceId, string userId, ReviewFormModel model);

        Task<ReviewFormModel> GetReviewForEditAsync(Guid reviewId, string currentUserId, bool isAdmin);
        Task DeleteReviewAsync(Guid reviewId, string userId, bool isAdmin);
        Task<Review?> GetReviewByIdInternal(Guid reviewId);
        Task UpdateReviewAsync(Guid reviewId, string userId, ReviewFormModel model, bool isAdmin);
    }
}
