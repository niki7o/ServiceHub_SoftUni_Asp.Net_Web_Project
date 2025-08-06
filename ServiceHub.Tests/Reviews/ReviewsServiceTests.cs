using Microsoft.AspNetCore.Identity;
using Moq;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using ServiceHub.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging; // Add this for ILogger mock

namespace ServiceHub.Tests.Reviews
{
    public class ReviewsServiceTests
    {
        private readonly Mock<IRepository<Review>> _mockReviewRepository;
        private readonly Mock<IRepository<Service>> _mockServiceRepository;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<ReviewsService>> _mockLogger; // Mock for ILogger
        private readonly ReviewsService _service;

        public ReviewsServiceTests()
        {
            _mockReviewRepository = new Mock<IRepository<Review>>();
            _mockServiceRepository = new Mock<IRepository<Service>>();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<ReviewsService>>(); // Initialize logger mock

            _service = new ReviewsService(
                _mockReviewRepository.Object,
                _mockServiceRepository.Object,
                _mockUserManager.Object,
                _mockLogger.Object); // Pass logger mock
        }

        [Fact]
        public async Task AddReviewAsync_ShouldAddReviewSuccessfully()
        {
            var serviceId = Guid.NewGuid();
            var userId = "test_user_id";
            var model = new ReviewFormModel { Rating = 5, Comment = "Great service!" };
            var service = new Service { Id = serviceId, IsTemplate = false, IsApproved = true };
            var user = new ApplicationUser { Id = userId };

            _mockServiceRepository.Setup(r => r.GetByIdAsync(serviceId)).ReturnsAsync(service);
            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockReviewRepository.Setup(r => r.AddAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);
            _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _service.AddReviewAsync(serviceId, userId, model);

            _mockReviewRepository.Verify(r => r.AddAsync(It.Is<Review>(rev =>
                rev.ServiceId == serviceId &&
                rev.UserId == userId &&
                rev.Rating == model.Rating &&
                rev.Comment == model.Comment)), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddReviewAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var serviceId = Guid.NewGuid();
            var userId = "test_user_id";
            var model = new ReviewFormModel { Rating = 5, Comment = "Great service!" };

            _mockServiceRepository.Setup(r => r.GetByIdAsync(serviceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddReviewAsync(serviceId, userId, model));
            Assert.Equal("Service not found.", exception.Message);
        }

        [Fact]
        public async Task AddReviewAsync_ShouldThrowInvalidOperationException_WhenServiceIsUnapprovedTemplate()
        {
            var serviceId = Guid.NewGuid();
            var userId = "test_user_id";
            var model = new ReviewFormModel { Rating = 5, Comment = "Great service!" };
            var service = new Service { Id = serviceId, IsTemplate = true, IsApproved = false }; // Unapproved template
            var user = new ApplicationUser { Id = userId };

            _mockServiceRepository.Setup(r => r.GetByIdAsync(serviceId)).ReturnsAsync(service);
            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddReviewAsync(serviceId, userId, model));
            Assert.Equal("Не може да оставяте ревюта за неодобрени шаблони.", exception.Message);
        }

        [Fact]
        public async Task AddReviewAsync_ShouldThrowArgumentException_WhenUserNotFound()
        {
            var serviceId = Guid.NewGuid();
            var userId = "test_user_id";
            var model = new ReviewFormModel { Rating = 5, Comment = "Great service!" };
            var service = new Service { Id = serviceId, IsTemplate = false, IsApproved = true };

            _mockServiceRepository.Setup(r => r.GetByIdAsync(serviceId)).ReturnsAsync(service);
            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddReviewAsync(serviceId, userId, model));
            Assert.Equal("User not found.", exception.Message);
        }

        [Fact]
        public async Task GetReviewForEditAsync_ShouldReturnModel_WhenUserIsOwner()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            var review = new Review { Id = reviewId, UserId = currentUserId, Rating = 4, Comment = "Good." };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

            var result = await _service.GetReviewForEditAsync(reviewId, currentUserId, false);

            Assert.NotNull(result);
            Assert.Equal(review.Rating, result.Rating);
            Assert.Equal(review.Comment, result.Comment);
        }

        [Fact]
        public async Task GetReviewForEditAsync_ShouldReturnModel_WhenUserIsAdmin()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "admin_user_id";
            var review = new Review { Id = reviewId, UserId = "other_user_id", Rating = 4, Comment = "Good." };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

            var result = await _service.GetReviewForEditAsync(reviewId, currentUserId, true);

            Assert.NotNull(result);
            Assert.Equal(review.Rating, result.Rating);
            Assert.Equal(review.Comment, result.Comment);
        }

        [Fact]
        public async Task GetReviewForEditAsync_ShouldThrowUnauthorizedException_WhenUserIsNotOwnerAndNotAdmin()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            var otherUserId = "other_user_id";
            var review = new Review { Id = reviewId, UserId = otherUserId, Rating = 4, Comment = "Good." };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetReviewForEditAsync(reviewId, currentUserId, false));
            Assert.Equal("You are not authorized to edit this review.", exception.Message);
        }

        [Fact]
        public async Task GetReviewForEditAsync_ShouldThrowArgumentException_WhenReviewNotFound()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetReviewForEditAsync(reviewId, currentUserId, false));
            Assert.Equal("Review not found.", exception.Message);
        }

        [Fact]
        public async Task UpdateReviewAsync_ShouldUpdateReview_WhenUserIsOwner()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            var existingReview = new Review { Id = reviewId, UserId = currentUserId, Rating = 4, Comment = "Good." };
            var model = new ReviewFormModel { Rating = 5, Comment = "Updated comment." };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);
            _mockReviewRepository.Setup(r => r.Update(It.IsAny<Review>()));
            _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _service.UpdateReviewAsync(reviewId, currentUserId, model, false);

            _mockReviewRepository.Verify(r => r.Update(It.Is<Review>(rev =>
                rev.Id == reviewId &&
                rev.Rating == model.Rating &&
                rev.Comment == model.Comment)), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateReviewAsync_ShouldUpdateReview_WhenUserIsAdmin()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "admin_user_id";
            var existingReview = new Review { Id = reviewId, UserId = "other_user_id", Rating = 4, Comment = "Good." };
            var model = new ReviewFormModel { Rating = 5, Comment = "Updated comment." };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);
            _mockReviewRepository.Setup(r => r.Update(It.IsAny<Review>()));
            _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _service.UpdateReviewAsync(reviewId, currentUserId, model, true);

            _mockReviewRepository.Verify(r => r.Update(It.Is<Review>(rev =>
                rev.Id == reviewId &&
                rev.Rating == model.Rating &&
                rev.Comment == model.Comment)), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateReviewAsync_ShouldThrowUnauthorizedException_WhenUserIsNotOwnerAndNotAdmin()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            var otherUserId = "other_user_id";
            var existingReview = new Review { Id = reviewId, UserId = otherUserId, Rating = 4, Comment = "Good." };
            var model = new ReviewFormModel { Rating = 5, Comment = "Updated comment." };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateReviewAsync(reviewId, currentUserId, model, false));
            Assert.Equal("You are not authorized to update this review.", exception.Message);
        }

        [Fact]
        public async Task UpdateReviewAsync_ShouldThrowArgumentException_WhenReviewNotFound()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            var model = new ReviewFormModel { Rating = 5, Comment = "Updated comment." };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateReviewAsync(reviewId, currentUserId, model, false));
            Assert.Equal("Review not found.", exception.Message);
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldDeleteReview_WhenUserIsOwner()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            var existingReview = new Review { Id = reviewId, UserId = currentUserId };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);
            _mockReviewRepository.Setup(r => r.Delete(It.IsAny<Review>()));
            _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _service.DeleteReviewAsync(reviewId, currentUserId, false);

            _mockReviewRepository.Verify(r => r.Delete(existingReview), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldDeleteReview_WhenUserIsAdmin()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "admin_user_id";
            var otherUserId = "other_user_id";
            var existingReview = new Review { Id = reviewId, UserId = otherUserId };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);
            _mockReviewRepository.Setup(r => r.Delete(It.IsAny<Review>()));
            _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.FromResult(1));

            await _service.DeleteReviewAsync(reviewId, currentUserId, true);

            _mockReviewRepository.Verify(r => r.Delete(existingReview), Times.Once);
            _mockReviewRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldThrowUnauthorizedException_WhenUserIsNotOwnerAndNotAdmin()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            var otherUserId = "other_user_id";
            var existingReview = new Review { Id = reviewId, UserId = otherUserId };

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteReviewAsync(reviewId, currentUserId, false));
            Assert.Equal("You are not authorized to delete this review.", exception.Message);
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldThrowArgumentException_WhenReviewNotFound()
        {
            var reviewId = Guid.NewGuid();
            var currentUserId = "test_user_id";

            _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteReviewAsync(reviewId, currentUserId, false));
            Assert.Equal("Review not found.", exception.Message);
        }
    }
}
