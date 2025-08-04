using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ReviewsService> _logger;

        public ReviewsService(
            IRepository<Review> reviewRepository,
            IRepository<Service> serviceRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<ReviewsService> logger)
        {
            _reviewRepository = reviewRepository;
            _serviceRepository = serviceRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task AddReviewAsync(Guid serviceId, string userId, ReviewFormModel model)
        {
            _logger.LogInformation($"AddReviewAsync: Опит за добавяне на ревю за ServiceId: {serviceId} от UserId: {userId}");

            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                _logger.LogWarning($"AddReviewAsync: Услуга с ID {serviceId} не е намерена.");
                throw new ArgumentException("Service not found.");
            }

            if (service.IsTemplate && !service.IsApproved)
            {
                _logger.LogWarning($"AddReviewAsync: Не може да оставяте ревюта за неодобрени шаблони (ServiceId: {serviceId}).");
                throw new InvalidOperationException("Не може да оставяте ревюта за неодобрени шаблони.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"AddReviewAsync: Потребител с ID {userId} не е намерен.");
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
            _logger.LogInformation($"AddReviewAsync: Ревю за ServiceId: {serviceId} от UserId: {userId} успешно добавено.");
        }

        public async Task<ReviewFormModel> GetReviewForEditAsync(Guid reviewId, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation($"GetReviewForEditAsync: Извличане на ревю с ID: {reviewId} за редактиране от потребител: {currentUserId}. Администратор: {isAdmin}");

            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                _logger.LogWarning($"GetReviewForEditAsync: Ревю с ID {reviewId} не е намерено.");
                throw new ArgumentException("Review not found.");
            }

           
            if (review.UserId != currentUserId && !isAdmin)
            {
                _logger.LogWarning($"GetReviewForEditAsync: Потребител {currentUserId} не е автор на ревю {reviewId} и не е администратор. Неоторизиран достъп.");
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
            _logger.LogInformation($"UpdateReviewAsync: Опит за обновяване на ревю с ID: {reviewId} от потребител: {currentUserId}. Администратор: {isAdmin}");

            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                _logger.LogWarning($"UpdateReviewAsync: Ревю с ID {reviewId} не е намерено.");
                throw new ArgumentException("Review not found.");
            }

            // Условието е "ако не си автор И не си админ, тогава не си оторизиран"
            if (review.UserId != currentUserId && !isAdmin)
            {
                _logger.LogWarning($"UpdateReviewAsync: Потребител {currentUserId} не е автор на ревю {reviewId} и не е администратор. Неоторизиран опит за обновяване.");
                throw new UnauthorizedAccessException("You are not authorized to update this review.");
            }

            review.Rating = model.Rating;
            review.Comment = model.Comment;
            review.ModifiedOn = DateTime.UtcNow;

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync();
            _logger.LogInformation($"UpdateReviewAsync: Ревю с ID {reviewId} успешно обновено.");
        }

        public async Task DeleteReviewAsync(Guid reviewId, string currentUserId, bool isAdmin)
        {
            _logger.LogInformation($"DeleteReviewAsync: Опит за изтриване на ревю с ID: {reviewId} от потребител: {currentUserId}. Администратор: {isAdmin}");

            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                _logger.LogWarning($"DeleteReviewAsync: Ревю с ID {reviewId} не е намерено.");
                throw new ArgumentException("Review not found.");
            }

         
            if (review.UserId != currentUserId && !isAdmin)
            {
                _logger.LogWarning($"DeleteReviewAsync: Потребител {currentUserId} не е автор на ревю {reviewId} и не е администратор. Неоторизиран опит за изтриване.");
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");
            }

            _reviewRepository.Delete(review);
            await _reviewRepository.SaveChangesAsync();
            _logger.LogInformation($"DeleteReviewAsync: Ревю с ID {reviewId} успешно изтрито.");
        }

        public async Task<Review?> GetReviewByIdInternal(Guid reviewId)
        {
            _logger.LogInformation($"GetReviewByIdInternal: Извличане на ревю с ID: {reviewId} за вътрешна употреба.");
            return await _reviewRepository.GetByIdAsync(reviewId);
        }

        public async Task<IEnumerable<ReviewViewModel>> GetReviewsByUserIdAsync(string userId)
        {
            _logger.LogInformation($"GetReviewsByUserIdAsync: Извличане на ревюта за потребител: {userId}");
            return await _reviewRepository.AllAsNoTracking()
                .Where(r => r.UserId == userId)
                .Include(r => r.Service)
                    .ThenInclude(s => s.Category)
                .Include(r => r.User)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    ServiceId = r.ServiceId,
                    ServiceName = r.Service.Title,
                    UserName = r.User.UserName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn,
                    IsAuthor = true 
                })
                .ToListAsync();
        }
    }
}
