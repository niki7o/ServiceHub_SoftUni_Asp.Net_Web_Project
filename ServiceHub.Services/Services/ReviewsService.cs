using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services
{
    public class ReviewsService : IReviewService
    {
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Service> _serviceRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsService(
            IRepository<Review> reviewRepository,
            IRepository<Service> serviceRepository,
            UserManager<ApplicationUser> userManager)
        {
            _reviewRepository = reviewRepository;
            _serviceRepository = serviceRepository;
            _userManager = userManager;
        }

        public async Task AddReviewAsync(Guid serviceId, string userId, ReviewFormModel model)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var review = new Review
            {
                ServiceId = serviceId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedOn = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();
        }

        public async Task<ReviewFormModel> GetReviewForEditAsync(Guid reviewId, string currentUserId, bool isAdmin)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new ArgumentException("Review not found.");
            }

           
            if (review.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this review.");
            }

            return new ReviewFormModel
            {
                Rating = review.Rating,
                Comment = review.Comment
            };
        }

        public async Task UpdateReviewAsync(Guid reviewId, string currentUserId, ReviewFormModel model, bool isAdmin)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new ArgumentException("Review not found.");
            }

            if (review.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this review.");
            }

            review.Rating = model.Rating;
            review.Comment = model.Comment;
            review.ModifiedOn = DateTime.UtcNow; 

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync();
        }

        public async Task DeleteReviewAsync(Guid reviewId, string currentUserId, bool isAdmin)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new ArgumentException("Review not found.");
            }

            
            if (review.UserId != currentUserId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");
            }

            _reviewRepository.Delete(review);
            await _reviewRepository.SaveChangesAsync();
        }

        public async Task<Review?> GetReviewByIdInternal(Guid reviewId)
        {
            return await _reviewRepository.GetByIdAsync(reviewId);
        }
    
}
}
