﻿//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.Extensions.Logging;
//using Moq;
//using ServiceHub.Areas.Admin.Controllers;
//using ServiceHub.Areas.Admin.Models;
//using ServiceHub.Data.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace ServiceHub.Tests
//{
//    public class AdminControllerTests
//    {
//        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
//        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
//        private readonly Mock<ILogger<AdminController>> _mockLogger;
//        private readonly AdminController _controller;
//        private readonly Mock<ITempDataDictionary> _mockTempData;

//        public AdminControllerTests()
//        {
//            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
//            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
//                userStoreMock.Object, null, null, null, null, null, null, null, null);

//            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
//            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
//                roleStoreMock.Object, null, null, null, null);

//            _mockLogger = new Mock<ILogger<AdminController>>();
//            _controller = new AdminController(_mockUserManager.Object, _mockRoleManager.Object, _mockLogger.Object);

//            _mockTempData = new Mock<ITempDataDictionary>();
//            _controller.TempData = _mockTempData.Object;

           
//            var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
//            {
//                new Claim(ClaimTypes.NameIdentifier, "admin_user_id"),
//                new Claim(ClaimTypes.Name, "adminuser")
//            }, "mock"));
//            _controller.ControllerContext = new ControllerContext
//            {
//                HttpContext = new DefaultHttpContext { User = adminUser }
//            };
//        }

//        [Fact]
//        public async Task AllUsers_ReturnsViewWithUserViewModels()
//        {
          
//            var users = new List<ApplicationUser>
//            {
//                new ApplicationUser { Id = "user1", UserName = "UserOne", Email = "user1@example.com" },
//                new ApplicationUser { Id = "user2", UserName = "UserTwo", Email = "user2@example.com" }
//            }.AsQueryable();

//            _mockUserManager.Setup(u => u.Users).Returns(users);
//            _mockUserManager.Setup(u => u.GetRolesAsync(It.Is<ApplicationUser>(au => au.Id == "user1"))).ReturnsAsync(new List<string> { "User" });
//            _mockUserManager.Setup(u => u.GetRolesAsync(It.Is<ApplicationUser>(au => au.Id == "user2"))).ReturnsAsync(new List<string> { "BusinessUser" });

//            var result = await _controller.AllUsers();

           
//            var viewResult = Assert.IsType<ViewResult>(result);
//            var model = Assert.IsType<List<UserViewModel>>(viewResult.Model);
//            Assert.Equal(2, model.Count);
//            Assert.Contains(model, u => u.UserName == "UserOne" && u.Roles.Contains("User"));
//            Assert.Contains(model, u => u.UserName == "UserTwo" && u.Roles.Contains("BusinessUser"));
//        }

//        [Fact]
//        public async Task DemoteBusinessUser_RedirectsToAllUsers_OnSuccess()
//        {
            
//            var userIdToDemote = "business_user_id";
//            var user = new ApplicationUser { Id = userIdToDemote, UserName = "BusinessUser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(userIdToDemote)).ReturnsAsync(user);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("admin_user_id"); 
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(true);
//            _mockUserManager.Setup(u => u.RemoveFromRoleAsync(user, "BusinessUser")).ReturnsAsync(IdentityResult.Success);
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "User")).ReturnsAsync(false);
//            _mockRoleManager.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);
//            _mockUserManager.Setup(u => u.AddToRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);

            
//            var result = await _controller.DemoteBusinessUser(userIdToDemote);

       
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["SuccessMessage"] = $"Потребител '{user.UserName}' успешно демоутнат до User.");
//            _mockUserManager.Verify(u => u.RemoveFromRoleAsync(user, "BusinessUser"), Times.Once);
//            _mockUserManager.Verify(u => u.AddToRoleAsync(user, "User"), Times.Once);
//        }

//        [Fact]
//        public async Task DemoteBusinessUser_ReturnsErrorMessage_WhenIdIsNullOrEmpty()
//        {
            
//            var result = await _controller.DemoteBusinessUser(null);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Невалиден потребителски идентификатор.");
//        }

//        [Fact]
//        public async Task DemoteBusinessUser_ReturnsErrorMessage_WhenUserNotFound()
//        {
           
//            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            
//            var result = await _controller.DemoteBusinessUser("non_existent_id");

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Потребителят не е намерен.");
//        }

//        [Fact]
//        public async Task DemoteBusinessUser_ReturnsErrorMessage_WhenDemotingSelf()
//        {
            
//            var adminUserId = "admin_user_id";
//            var adminUser = new ApplicationUser { Id = adminUserId, UserName = "adminuser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(adminUserId)).ReturnsAsync(adminUser);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(adminUserId);

//            var result = await _controller.DemoteBusinessUser(adminUserId);

           
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Не можете да демоутнете собствения си акаунт.");
//        }

//        [Fact]
//        public async Task DemoteBusinessUser_ReturnsWarningMessage_WhenUserIsNotBusinessUser()
//        {
           
//            var userId = "normal_user_id";
//            var user = new ApplicationUser { Id = userId, UserName = "NormalUser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("admin_user_id");
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(false); 

//            var result = await _controller.DemoteBusinessUser(userId);

           
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["WarningMessage"] = "Потребителят не е BusinessUser и не може да бъде демоутнат до User.");
//        }

//        [Fact]
//        public async Task PromoteToBusinessUser_RedirectsToAllUsers_OnSuccess()
//        {
            
//            var userIdToPromote = "normal_user_id";
//            var user = new ApplicationUser { Id = userIdToPromote, UserName = "NormalUser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(userIdToPromote)).ReturnsAsync(user);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("admin_user_id");
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(false);
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "User")).ReturnsAsync(true);
//            _mockUserManager.Setup(u => u.RemoveFromRoleAsync(user, "User")).ReturnsAsync(IdentityResult.Success);
//            _mockRoleManager.Setup(r => r.RoleExistsAsync("BusinessUser")).ReturnsAsync(true);
//            _mockUserManager.Setup(u => u.AddToRoleAsync(user, "BusinessUser")).ReturnsAsync(IdentityResult.Success);

           
//            var result = await _controller.PromoteToBusinessUser(userIdToPromote);

           
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["SuccessMessage"] = $"Потребител '{user.UserName}' успешно промоутнат до BusinessUser.");
//            _mockUserManager.Verify(u => u.RemoveFromRoleAsync(user, "User"), Times.Once);
//            _mockUserManager.Verify(u => u.AddToRoleAsync(user, "BusinessUser"), Times.Once);
//        }

//        [Fact]
//        public async Task PromoteToBusinessUser_ReturnsErrorMessage_WhenIdIsNullOrEmpty()
//        {
//            var result = await _controller.PromoteToBusinessUser(null);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Невалиден потребителски идентификатор.");
//        }

//        [Fact]
//        public async Task PromoteToBusinessUser_ReturnsErrorMessage_WhenUserNotFound()
//        {
            
//            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

          
//            var result = await _controller.PromoteToBusinessUser("non_existent_id");

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Потребителят не е намерен.");
//        }

//        [Fact]
//        public async Task PromoteToBusinessUser_ReturnsErrorMessage_WhenPromotingSelf()
//        {
            
//            var adminUserId = "admin_user_id";
//            var adminUser = new ApplicationUser { Id = adminUserId, UserName = "adminuser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(adminUserId)).ReturnsAsync(adminUser);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(adminUserId);

            
//            var result = await _controller.PromoteToBusinessUser(adminUserId);

           
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Не можете да промоутнете собствения си акаунт.");
//        }

//        [Fact]
//        public async Task PromoteToBusinessUser_ReturnsWarningMessage_WhenUserIsAlreadyBusinessOrAdmin()
//        {
//            var businessUserId = "business_user_id";
//            var businessUser = new ApplicationUser { Id = businessUserId, UserName = "BusinessUser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(businessUserId)).ReturnsAsync(businessUser);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("admin_user_id");
//            _mockUserManager.Setup(u => u.IsInRoleAsync(businessUser, "BusinessUser")).ReturnsAsync(true); 

           
//            var result = await _controller.PromoteToBusinessUser(businessUserId);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["WarningMessage"] = "Потребителят вече е BusinessUser или Admin и не може да бъде промоутнат.");
//        }

//        [Fact]
//        public async Task DeleteUser_RedirectsToAllUsers_OnSuccess()
//        {
            
//            var userIdToDelete = "user_to_delete_id";
//            var user = new ApplicationUser { Id = userIdToDelete, UserName = "UserToDelete" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(userIdToDelete)).ReturnsAsync(user);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("admin_user_id");
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
//            _mockUserManager.Setup(u => u.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(false);
//            _mockUserManager.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

         
//            var result = await _controller.DeleteUser(userIdToDelete);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["SuccessMessage"] = $"Потребител '{user.UserName}' успешно изтрит.");
//            _mockUserManager.Verify(u => u.DeleteAsync(user), Times.Once);
//        }

//        [Fact]
//        public async Task DeleteUser_ReturnsErrorMessage_WhenIdIsNullOrEmpty()
//        {
           
//            var result = await _controller.DeleteUser(null);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Невалиден потребителски идентификатор.");
//        }

//        [Fact]
//        public async Task DeleteUser_ReturnsErrorMessage_WhenUserNotFound()
//        {
            
//            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

           
//            var result = await _controller.DeleteUser("non_existent_id");

           
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Потребителят не е намерен.");
//        }

//        [Fact]
//        public async Task DeleteUser_ReturnsErrorMessage_WhenDeletingSelf()
//        {
            
//            var adminUserId = "admin_user_id";
//            var adminUser = new ApplicationUser { Id = adminUserId, UserName = "adminuser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(adminUserId)).ReturnsAsync(adminUser);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(adminUserId);

            
//            var result = await _controller.DeleteUser(adminUserId);

          
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Не можете да изтриете собствения си акаунт.");
//        }

//        [Fact]
//        public async Task DeleteUser_ReturnsErrorMessage_WhenDeletingAdminOrBusinessUser()
//        {
            
//            var adminOrBusinessUserId = "privileged_user_id";
//            var privilegedUser = new ApplicationUser { Id = adminOrBusinessUserId, UserName = "PrivilegedUser" };

//            _mockUserManager.Setup(u => u.FindByIdAsync(adminOrBusinessUserId)).ReturnsAsync(privilegedUser);
//            _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("admin_user_id");
//            _mockUserManager.Setup(u => u.IsInRoleAsync(privilegedUser, "Admin")).ReturnsAsync(true); 

            
//            var result = await _controller.DeleteUser(adminOrBusinessUserId);

            
//            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
//            Assert.Equal(nameof(AdminController.AllUsers), redirectResult.ActionName);
//            _mockTempData.VerifySet(td => td["ErrorMessage"] = "Не можете да изтриете Администратор или BusinessUser директно. Моля, първо понижете ролята им.");
//        }
//    }
//}
