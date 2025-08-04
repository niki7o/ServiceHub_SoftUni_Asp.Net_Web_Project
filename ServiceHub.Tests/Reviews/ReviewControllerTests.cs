//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Moq;
//using ServiceHub.Controllers;
//using ServiceHub.Core.Models.Reviews;
//using ServiceHub.Core.Models.Service;
//using ServiceHub.Data.Models;
//using ServiceHub.Services.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace ServiceHub.Tests.Reviews
//{
//    public class ReviewControllerTests
//    {
//        private readonly Mock<IReviewService> _mockReviewService;
//        private readonly Mock<IServiceService> _mockServiceService;
//        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
//        private readonly ReviewController _controller;
//        private readonly Mock<ITempDataDictionary> _mockTempData;

//        public ReviewControllerTests()
//        {
//            _mockReviewService = new Mock<IReviewService>();
//            _mockServiceService = new Mock<IServiceService>();
//            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
//            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
//                userStoreMock.Object, null, null, null, null, null, null, null, null);

//            _controller = new ReviewController(
//                _mockReviewService.Object,
//                _mockServiceService.Object,
//                _mockUserManager.Object);

//            _mockTempData = new Mock<ITempDataDictionary>();
//            _controller.TempData = _mockTempData.Object;

//            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//            {
//                new Claim(ClaimTypes.NameIdentifier, "test_user_id")
//            }, "mock"));
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext { User = user }
//            };
//        }

//        [Fact]
//        public async Task CreateReview_ReturnsViewWithModel_WhenServiceExists()
//        {
            
//            var serviceId = Guid.NewGuid();
          
//            var service = new ServiceViewModel { Id = serviceId, Title = "Test Service" };
//            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<string>())).ReturnsAsync(service);

           
//            var result = await _controller.CreateReview(serviceId);

//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.IsType<ReviewFormModel>(viewResult.Model);
//            Assert.Equal(serviceId, _controller.ViewBag.ServiceId);
//            Assert.Equal("Test Service", _controller.ViewBag.ServiceTitle);
//        }

//        [Fact]
//        public async Task CreateReview_RedirectsToAllServices_WhenServiceDoesNotExist()
//        {
            
//            var serviceId = Guid.NewGuid();
           
//            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<string>())).ReturnsAsync((ServiceViewModel)null);

         
//            var result = await _controller.CreateReview(serviceId);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("All", redirectResult.ActionName);
//            Assert.Equal("Service", redirectResult.ControllerName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Service not found.");
//        }

//        [Fact]
//        public async Task AddReview_RedirectsToDetailsService_OnSuccess()
//        {
            
//            var serviceId = Guid.NewGuid();
//            var model = new ReviewFormModel { Rating = 5, Comment = "Test comment" };
//            _controller.ModelState.Clear();
//            _mockReviewService.Setup(s => s.AddReviewAsync(serviceId, It.IsAny<string>(), model)).Returns(Task.CompletedTask);

            
//            var result = await _controller.AddReview(serviceId, model);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Details", redirectResult.ActionName);
//            Assert.Equal("Service", redirectResult.ControllerName);
//            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
//            _mockTempData.VerifySet(td => td["SuccessMessage"] = "Review added successfully!");
//        }

//        [Fact]
//        public async Task AddReview_ReturnsRedirectToCreateReview_OnInvalidModelState()
//        {
            
//            var serviceId = Guid.NewGuid();
//            var model = new ReviewFormModel { Rating = 0, Comment = "" }; 
//            _controller.ModelState.AddModelError("Rating", "Rating is required");

//            var result = await _controller.AddReview(serviceId, model);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(ReviewController.CreateReview), redirectResult.ActionName);
//            Assert.Equal(serviceId, redirectResult.RouteValues["serviceId"]);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Failed to add review. Please check your input.");
//        }

//        [Fact]
//        public async Task AddReview_ReturnsUnauthorized_WhenUserNotLoggedIn()
//        {
          
//            var serviceId = Guid.NewGuid();
//            var model = new ReviewFormModel();
//            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(); 

            
//            var result = await _controller.AddReview(serviceId, model);

            
//            Assert.IsType<UnauthorizedResult>(result);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "You must be logged in to leave a review.");
//        }

//        [Fact]
//        public async Task EditReviewGet_ReturnsViewWithModel_OnSuccess()
//        {
            
//            var reviewId = Guid.NewGuid();
//            var serviceId = Guid.NewGuid();
//            var reviewFormModel = new ReviewFormModel { Rating = 4, Comment = "Test edit" };
//            var reviewEntity = new Review { Id = reviewId, UserId = "test_user_id", ServiceId = serviceId };
           
//            var serviceModel = new ServiceViewModel { Id = serviceId, Title = "Test Service" };

//            _mockReviewService.Setup(s => s.GetReviewForEditAsync(reviewId, "test_user_id", false)).ReturnsAsync(reviewFormModel);
//            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
//            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, "test_user_id")).ReturnsAsync(serviceModel);
//            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser());
//            _mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin")).ReturnsAsync(false);

            
//            var result = await _controller.EditReview(reviewId);

           
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var model = Assert.IsType<ReviewFormModel>(viewResult.Model);
//            Assert.Equal(reviewFormModel.Rating, model.Rating);
//            Assert.Equal(reviewFormModel.Comment, model.Comment);
//            Assert.Equal(reviewId, _controller.ViewBag.ReviewId);
//            Assert.Equal(serviceId, _controller.ViewBag.ServiceId);
//        }

//        [Fact]
//        public async Task EditReviewPost_RedirectsToDetailsService_OnSuccess()
//        {
            
//            var reviewId = Guid.NewGuid();
//            var serviceId = Guid.NewGuid();
//            var model = new ReviewFormModel { Rating = 5, Comment = "Updated comment" };
//            var reviewEntity = new Review { Id = reviewId, UserId = "test_user_id", ServiceId = serviceId };

//            _controller.ModelState.Clear();
//            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
//            _mockReviewService.Setup(s => s.UpdateReviewAsync(reviewId, "test_user_id", model, false)).Returns(Task.CompletedTask);
//            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser());
//            _mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin")).ReturnsAsync(false);

            
//            var result = await _controller.EditReview(reviewId, model);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Details", redirectResult.ActionName);
//            Assert.Equal("Service", redirectResult.ControllerName);
//            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
//            _mockTempData.VerifySet(td => td["SuccessMessage"] = "Review updated successfully!");
//        }

//        [Fact]
//        public async Task EditReviewPost_ReturnsView_OnInvalidModelState()
//        {
           
//            var reviewId = Guid.NewGuid();
//            var serviceId = Guid.NewGuid();
//            var model = new ReviewFormModel { Rating = 0, Comment = "" }; 
//            var reviewEntity = new Review { Id = reviewId, UserId = "test_user_id", ServiceId = serviceId };
          
//            var serviceModel = new ServiceViewModel { Id = serviceId, Title = "Test Service" };

//            _controller.ModelState.AddModelError("Rating", "Rating is required");
//            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
//            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<string>())).ReturnsAsync(serviceModel);
//            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser());
//            _mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin")).ReturnsAsync(false);

            
//            var result = await _controller.EditReview(reviewId, model);

           
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var returnedModel = Assert.IsType<ReviewFormModel>(viewResult.Model);
//            Assert.Equal(model.Rating, returnedModel.Rating);
//            Assert.Equal(model.Comment, returnedModel.Comment);
//            Assert.Equal(reviewId, _controller.ViewBag.ReviewId);
//            Assert.Equal(serviceId, _controller.ViewBag.ServiceId);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Failed to update review. Please check your input.");
//        }


//        [Fact]
//        public async Task DeleteReview_RedirectsToDetailsService_OnSuccess()
//        {
           
//            var reviewId = Guid.NewGuid();
//            var serviceId = Guid.NewGuid();
//            var reviewEntity = new Review { Id = reviewId, UserId = "test_user_id", ServiceId = serviceId };

//            _mockReviewService.Setup(s => s.GetReviewByIdInternal(reviewId)).ReturnsAsync(reviewEntity);
//            _mockReviewService.Setup(s => s.DeleteReviewAsync(reviewId, "test_user_id", false)).Returns(Task.CompletedTask);
//            _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser());
//            _mockUserManager.Setup(u => u.IsInRoleAsync(It.IsAny<ApplicationUser>(), "Admin")).ReturnsAsync(false);


//            var result = await _controller.DeleteReview(reviewId);

           
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal("Details", redirectResult.ActionName);
//            Assert.Equal("Service", redirectResult.ControllerName);
//            Assert.Equal(serviceId, redirectResult.RouteValues["id"]);
//            _mockTempData.VerifySet(td => td["SuccessMessage"] = "Review deleted successfully!");
//        }
//    }
//}
