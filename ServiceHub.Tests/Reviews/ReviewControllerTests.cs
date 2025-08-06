using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using ServiceHub.Controllers;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;

namespace ServiceHub.Tests.Reviews
{
    public class ReviewControllerTests
    {
        private readonly Mock<IReviewService> _mockReviewService;
        private readonly Mock<IServiceService> _mockServiceService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<ReviewController>> _mockLogger;
        private readonly ReviewController _controller;

        public ReviewControllerTests()
        {
            _mockReviewService = new Mock<IReviewService>();
            _mockServiceService = new Mock<IServiceService>();
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mockLogger = new Mock<ILogger<ReviewController>>();

            _controller = new ReviewController(
                _mockReviewService.Object,
                _mockServiceService.Object,
                _mockUserManager.Object,
                _mockLogger.Object);
        }

        private void SetupUserContext(string userId, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testusername")
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            var tempData = new TempDataDictionary(httpContext, new Mock<ITempDataProvider>().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;

            var user = new ApplicationUser { Id = userId, UserName = "testusername" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin"))
                            .ReturnsAsync(roles.Contains("Admin"));
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "BusinessUser"))
                            .ReturnsAsync(roles.Contains("BusinessUser"));
        }

        private void SetupEmptyUserContext()
        {
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };
            var tempData = new TempDataDictionary(httpContext, new Mock<ITempDataProvider>().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                            .ReturnsAsync(false);
        }

        [Fact]
        public async Task CreateReview_ReturnsViewWithModel_WhenServiceExistsAndApproved()
        {
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var service = new ServiceViewModel { Id = serviceId, Title = "Test Service", IsTemplate = false, IsApproved = true };
 
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, currentUserId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);

            var result = await _controller.CreateReview(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ReviewFormModel>(viewResult.Model);
            Assert.Equal(serviceId, _controller.ViewBag.ServiceId);
            Assert.Equal("Test Service", _controller.ViewBag.ServiceTitle);
        }

        [Fact]
        public async Task CreateReview_RedirectsToAllServices_WhenServiceDoesNotExist()
        {
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

      
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, currentUserId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((ServiceViewModel)null);

            var result = await _controller.CreateReview(serviceId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectResult.ActionName);
            Assert.Equal("Service", redirectResult.ControllerName);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Услугата не е намерена.", errorMessage);
        }

        [Fact]
        public async Task CreateReview_RedirectsToDetailsService_WhenServiceIsUnapprovedTemplate()
        {
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var service = new ServiceViewModel { Id = serviceId, Title = "Test Template", IsTemplate = true, IsApproved = false };
          
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, currentUserId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);

            var result = await _controller.CreateReview(serviceId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Service", redirectResult.ControllerName);
            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Не може да добавяте ревюта за шаблони или неодобрени услуги.", errorMessage);
        }

        [Fact]
        public async Task AddReview_RedirectsToDetailsService_OnSuccess()
        {
            var serviceId = Guid.NewGuid();
            var userId = "test_user_id";
            var model = new ReviewFormModel { Rating = 5, Comment = "Test comment" };
            SetupUserContext(userId, "User");

            _controller.ModelState.Clear();
            _mockReviewService.Setup(s => s.AddReviewAsync(serviceId, userId, model)).Returns(Task.CompletedTask);

            var result = await _controller.AddReview(serviceId, model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Service", redirectResult.ControllerName);
            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
            _controller.TempData.TryGetValue("SuccessMessage", out var successMessage);
            Assert.Equal("Ревюто е успешно добавено!", successMessage);
        }

        [Fact]
        public async Task AddReview_ReturnsView_OnInvalidModelState()
        {
            var serviceId = Guid.NewGuid();
            var userId = "test_user_id";
            var model = new ReviewFormModel { Rating = 0, Comment = "" };
            SetupUserContext(userId, "User");

            _controller.ModelState.AddModelError("Rating", "Rating is required");
            var service = new ServiceViewModel { Id = serviceId, Title = "Test Service" };
          
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, userId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);

            var result = await _controller.AddReview(serviceId, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ReviewFormModel>(viewResult.Model);
            Assert.Equal(serviceId, _controller.ViewBag.ServiceId);
            Assert.Equal("Test Service", _controller.ViewBag.ServiceTitle);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Моля, попълнете всички задължителни полета коректно.", errorMessage);
        }

        [Fact]
        public async Task AddReview_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var serviceId = Guid.NewGuid();
            var model = new ReviewFormModel();
            SetupEmptyUserContext();

            var result = await _controller.AddReview(serviceId, model);

            Assert.IsType<UnauthorizedResult>(result);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Трябва да сте логнати, за да оставите ревю.", errorMessage);
        }

        [Fact]
        public async Task EditReviewGet_ReturnsViewWithModel_OnSuccess()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var reviewFormModel = new ReviewFormModel { Rating = 4, Comment = "Test edit" };
            var reviewEntity = new Review { Id = reviewId, UserId = currentUserId, ServiceId = serviceId };
            var serviceModel = new ServiceViewModel { Id = serviceId, Title = "Test Service" };

            _mockReviewService.Setup(s => s.GetReviewForEditAsync(reviewId, currentUserId, false)).ReturnsAsync(reviewFormModel);
            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
     
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, currentUserId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(serviceModel);

            var result = await _controller.EditReview(reviewId, serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReviewFormModel>(viewResult.Model);
            Assert.Equal(reviewFormModel.Rating, model.Rating);
            Assert.Equal(reviewFormModel.Comment, model.Comment);
            Assert.Equal(reviewId, _controller.ViewBag.ReviewId);
            Assert.Equal(serviceId, _controller.ViewBag.ServiceId);
        }

        [Fact]
        public async Task EditReviewGet_ReturnsRedirectToDetailsService_WhenReviewNotFound()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            _mockReviewService.Setup(s => s.GetReviewForEditAsync(reviewId, currentUserId, false)).ThrowsAsync(new ArgumentException("Review not found."));

            var result = await _controller.EditReview(reviewId, serviceId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Service", redirectResult.ControllerName);
            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Review not found.", errorMessage);
        }

        [Fact]
        public async Task EditReviewGet_ReturnsForbid_WhenUserNotAuthorized()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            _mockReviewService.Setup(s => s.GetReviewForEditAsync(reviewId, currentUserId, false)).ThrowsAsync(new UnauthorizedAccessException("You are not authorized to edit this review."));

            var result = await _controller.EditReview(reviewId, serviceId);

            Assert.IsType<ForbidResult>(result);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("You are not authorized to edit this review.", errorMessage);
        }

        [Fact]
        public async Task EditReviewPost_RedirectsToDetailsService_OnSuccess()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var model = new ReviewFormModel { Rating = 5, Comment = "Updated comment" };
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var reviewEntity = new Review { Id = reviewId, UserId = currentUserId, ServiceId = serviceId };

            _controller.ModelState.Clear();
            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
            _mockReviewService.Setup(s => s.UpdateReviewAsync(reviewId, currentUserId, model, false)).Returns(Task.CompletedTask);

            var result = await _controller.EditReview(reviewId, model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Service", redirectResult.ControllerName);
            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
            _controller.TempData.TryGetValue("SuccessMessage", out var successMessage);
            Assert.Equal("Ревюто е успешно обновено!", successMessage);
        }

        [Fact]
        public async Task EditReviewPost_ReturnsView_OnInvalidModelState()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var model = new ReviewFormModel { Rating = 0, Comment = "" };
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var reviewEntity = new Review { Id = reviewId, UserId = currentUserId, ServiceId = serviceId };
            var serviceModel = new ServiceViewModel { Id = serviceId, Title = "Test Service" };

            _controller.ModelState.AddModelError("Rating", "Rating is required");
            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
        
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, currentUserId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(serviceModel);

            var result = await _controller.EditReview(reviewId, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<ReviewFormModel>(viewResult.Model);
            Assert.Equal(model.Rating, returnedModel.Rating);
            Assert.Equal(model.Comment, returnedModel.Comment);
            Assert.Equal(reviewId, _controller.ViewBag.ReviewId);
            Assert.Equal(serviceId, _controller.ViewBag.ServiceId);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Моля, попълнете всички задължителни полета коректно.", errorMessage);
        }

        [Fact]
        public async Task EditReviewPost_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var reviewId = Guid.NewGuid();
            var model = new ReviewFormModel();
            SetupEmptyUserContext();

            var result = await _controller.EditReview(reviewId, model);

            Assert.IsType<UnauthorizedResult>(result);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Трябва да сте логнати, за да редактирате ревю.", errorMessage);
        }

        [Fact]
        public async Task EditReviewPost_ReturnsForbid_WhenUserNotAuthorized()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var model = new ReviewFormModel { Rating = 5, Comment = "Updated comment" };
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var reviewEntity = new Review { Id = reviewId, UserId = "other_user_id", ServiceId = serviceId };

            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
            _mockReviewService.Setup(s => s.UpdateReviewAsync(reviewId, currentUserId, model, false)).ThrowsAsync(new UnauthorizedAccessException("You are not authorized to update this review."));

            var result = await _controller.EditReview(reviewId, model);

            Assert.IsType<ForbidResult>(result);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("You are not authorized to update this review.", errorMessage);
        }

        [Fact]
        public async Task DeleteReview_RedirectsToDetailsService_OnSuccess()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var reviewEntity = new Review { Id = reviewId, UserId = currentUserId, ServiceId = serviceId };

            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
            _mockReviewService.Setup(s => s.DeleteReviewAsync(reviewId, currentUserId, false)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteReview(reviewId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal("Service", redirectResult.ControllerName);
            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
            _controller.TempData.TryGetValue("SuccessMessage", out var successMessage);
            Assert.Equal("Ревюто е успешно изтрито.", successMessage);
        }

        [Fact]
        public async Task DeleteReview_ReturnsUnauthorized_WhenUserNotLoggedIn()
        {
            var reviewId = Guid.NewGuid();
            SetupEmptyUserContext();

            var result = await _controller.DeleteReview(reviewId);

            Assert.IsType<UnauthorizedResult>(result);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Трябва да сте логнати, за да изтриете ревю.", errorMessage);
        }

        [Fact]
        public async Task DeleteReview_ReturnsForbid_WhenUserNotAuthorized()
        {
            var reviewId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var currentUserId = "test_user_id";
            SetupUserContext(currentUserId, "User");

            var reviewEntity = new Review { Id = reviewId, UserId = "other_user_id", ServiceId = serviceId };

            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
            _mockReviewService.Setup(s => s.DeleteReviewAsync(reviewId, currentUserId, false)).ThrowsAsync(new UnauthorizedAccessException("You are not authorized to delete this review."));

            var result = await _controller.DeleteReview(reviewId);

            Assert.IsType<ForbidResult>(result);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("You are not authorized to delete this review.", errorMessage);
        }
    }
}
