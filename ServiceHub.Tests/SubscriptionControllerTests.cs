using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ServiceHub.Controllers;
using ServiceHub.Core.Models;
using ServiceHub.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.Controllers
{
    public class SubscriptionControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<ILogger<SubscriptionController>> _mockLogger;
        private readonly SubscriptionController _controller;

        public SubscriptionControllerTests()
        {
            
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

        
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

           
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                httpContextAccessorMock.Object,
                userClaimsPrincipalFactoryMock.Object,
                null, null, null, null);

            _mockLogger = new Mock<ILogger<SubscriptionController>>();

           
            _controller = new SubscriptionController(
                _mockUserManager.Object,
                _mockRoleManager.Object,
                _mockSignInManager.Object,
                _mockLogger.Object);

            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test_user_id"),
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task Subscribe_ReturnsOk_WhenNewUserSubscribesSuccessfully()
        {
            
            var request = new SubscribeRequestModel { ConfirmSubscription = true };
            var userId = "test_user_id";
            var user = new ApplicationUser { Id = userId, UserName = "testuser", IsBusiness = false };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);

            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(false); 
            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "User")).ReturnsAsync(true); 
            _mockUserManager.Setup(u => u.RemoveFromRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);
            _mockRoleManager.Setup(r => r.RoleExistsAsync("BusinessUser")).ReturnsAsync(true);
            _mockUserManager.Setup(u => u.AddToRoleAsync(user, "BusinessUser")).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mockSignInManager.Setup(s => s.RefreshSignInAsync(user)).Returns(Task.CompletedTask);

            
            var result = await _controller.Subscribe(request);

            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(okResult.Value));

            Assert.Contains("Поздравления! Вече сте Бизнес Потребител за 30 дни!", response["message"]);
            Assert.NotNull(response["expiresOn"]);
            Assert.True(user.IsBusiness);
            Assert.NotNull(user.BusinessExpiresOn);
            _mockUserManager.Verify(u => u.RemoveFromRoleAsync(user, "User"), Times.Once);
            _mockUserManager.Verify(u => u.AddToRoleAsync(user, "BusinessUser"), Times.Once);
            _mockUserManager.Verify(u => u.UpdateAsync(user), Times.Once);
            _mockSignInManager.Verify(s => s.RefreshSignInAsync(user), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Simulating successful payment for user {user.UserName} ({userId}).")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
           
        }

        [Fact]
        public async Task Subscribe_ReturnsOk_WhenUserRenewsSubscription()
        {

            var request = new SubscribeRequestModel { ConfirmSubscription = true };
            var userId = "test_user_id";
            var user = new ApplicationUser { Id = userId, UserName = "testuser", IsBusiness = true, BusinessExpiresOn = DateTime.UtcNow.AddDays(-10) };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(true); 
            _mockRoleManager.Setup(r => r.RoleExistsAsync("BusinessUser")).ReturnsAsync(true);
            _mockUserManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mockSignInManager.Setup(s => s.RefreshSignInAsync(user)).Returns(Task.CompletedTask);

           
            var result = await _controller.Subscribe(request);

            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(okResult.Value));

            Assert.Contains("Абонаментът Ви за Бизнес Потребител е успешно подновен за 30 дни!", response["message"]);
            Assert.NotNull(response["expiresOn"]);
            Assert.True(user.IsBusiness);
            Assert.True(user.BusinessExpiresOn > DateTime.UtcNow); 
            _mockUserManager.Verify(u => u.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never); 
            _mockUserManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never); 
            _mockUserManager.Verify(u => u.UpdateAsync(user), Times.Once);
            _mockSignInManager.Verify(s => s.RefreshSignInAsync(user), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"User {user.UserName} ({userId}) is already a BusinessUser. Attempting to renew subscription.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            
        }

        [Fact]
        public async Task Subscribe_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            
            _controller.ModelState.AddModelError("ConfirmSubscription", "Confirmation is required.");
            var request = new SubscribeRequestModel { ConfirmSubscription = false }; 

           
            var result = await _controller.Subscribe(request);

           
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey("ConfirmSubscription"));
            Assert.Contains("Confirmation is required.", (string[])modelState["ConfirmSubscription"]);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid model state for SubscribeRequestModel:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Subscribe_ReturnsBadRequest_WhenConfirmationNotGiven()
        {
          
            var request = new SubscribeRequestModel { ConfirmSubscription = false }; 

           
            var result = await _controller.Subscribe(request);

          
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(badRequestResult.Value));
            Assert.Contains("Моля, потвърдете абонамента си.", response["message"]);
        }

        [Fact]
        public async Task Subscribe_ReturnsUnauthorized_WhenUserIdNotFoundInClaims()
        {
           
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            var request = new SubscribeRequestModel { ConfirmSubscription = true };

            
            var result = await _controller.Subscribe(request);

            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(unauthorizedResult.Value));
            Assert.Contains("Неупълномощен достъп. Моля, влезте в профила си.", response["message"]);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Unauthorized attempt to subscribe: User ID not found in claims.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Subscribe_ReturnsNotFound_WhenUserNotFoundInUserManager()
        {
          
            var request = new SubscribeRequestModel { ConfirmSubscription = true };
            var userId = "non_existent_user_id";

          
            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser)null);

           
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

          
            var result = await _controller.Subscribe(request);

            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(notFoundResult.Value));
            Assert.Contains("Потребителят не е намерен.", response["message"]);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"User with ID {userId} not found during subscription attempt.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Subscribe_ReturnsInternalServerError_WhenBusinessRoleDoesNotExist()
        {
           
            var request = new SubscribeRequestModel { ConfirmSubscription = true };
            var userId = "test_user_id";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(false);
            _mockRoleManager.Setup(r => r.RoleExistsAsync("BusinessUser")).ReturnsAsync(false); 

          
            var result = await _controller.Subscribe(request);

            
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(statusCodeResult.Value));
            Assert.Contains("Възникна вътрешна грешка: Ролята 'BusinessUser' не е намерена.", response["message"]);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Role 'BusinessUser' does not exist. Please seed roles.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Subscribe_ReturnsInternalServerError_WhenAddToRoleFails()
        {
            
            var request = new SubscribeRequestModel { ConfirmSubscription = true };
            var userId = "test_user_id";
            var user = new ApplicationUser { Id = userId, UserName = "testuser" };

            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(false);
            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "User")).ReturnsAsync(true); 
            _mockUserManager.Setup(u => u.RemoveFromRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);
            _mockRoleManager.Setup(r => r.RoleExistsAsync("BusinessUser")).ReturnsAsync(true);
          
            _mockUserManager.Setup(u => u.AddToRoleAsync(user, "BusinessUser")).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Add role failed" }));

          
            var result = await _controller.Subscribe(request);

            
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(statusCodeResult.Value));
            Assert.Contains("Неуспешно активиране на абонамента. Моля, свържете се с поддръжката.", response["message"]);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Failed to add 'BusinessUser' role to {user.UserName}: Add role failed")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        
    }
}
