using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json.Linq;
using ServiceHub.Common;
using ServiceHub.Common.Enum;
using ServiceHub.Controllers;
using ServiceHub.Core.Models;
using ServiceHub.Core.Models.Service;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.Services
{
    public class ServiceControllerTests
    {
        private readonly Mock<IServiceService> _mockServiceService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IRepository<Favorite>> _mockFavoriteRepo;
        private readonly Mock<IRepository<Service>> _mockServiceRepository;
        private readonly Mock<IRepository<Category>> _mockCategoryRepository;
        private readonly Mock<ILogger<ServiceController>> _mockLogger;
        private readonly Mock<IServiceDispatcher> _mockServiceDispatcher;
        private readonly ServiceController _controller;

        private readonly List<ServiceViewModel> _serviceViewModels;
        private readonly List<Category> _categories;
        private readonly ApplicationUser _testUser;
        private readonly ClaimsPrincipal _userPrincipal;

        public ServiceControllerTests()
        {
            _mockLogger = new Mock<ILogger<ServiceController>>();
            _mockServiceDispatcher = new Mock<IServiceDispatcher>();
            _mockServiceService = new Mock<IServiceService>();
            _mockFavoriteRepo = new Mock<IRepository<Favorite>>();
            _mockServiceRepository = new Mock<IRepository<Service>>();
            _mockCategoryRepository = new Mock<IRepository<Category>>();

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            _controller = new ServiceController(
                _mockServiceDispatcher.Object,
                _mockServiceService.Object,
                _mockUserManager.Object,
                _mockFavoriteRepo.Object,
                _mockServiceRepository.Object,
                _mockCategoryRepository.Object,
                _mockLogger.Object
            );

            _categories = new List<Category>
            {
                new Category { Id = Guid.Parse("A0A0A0A0-A0A0-A0A0-A0A0-000000000001"), Name = "Документи", Description = "Документи", CreatedOn = DateTime.UtcNow },
                new Category { Id = Guid.Parse("B1B1B1B1-B1B1-B1B1-B1B1-000000000002"), Name = "Инструменти", Description = "Инструменти", CreatedOn = DateTime.UtcNow }
            };

            _serviceViewModels = new List<ServiceViewModel>
            {
                new ServiceViewModel { Id = Guid.NewGuid(), Title = "Service X", Description = "Desc X", AccessType = AccessType.Free, CategoryName = "Документи", ReviewCount = 0, AverageRating = 0, IsFavorite = false, ViewsCount = 0 },
                new ServiceViewModel { Id = Guid.NewGuid(), Title = "Service Y", Description = "Desc Y", AccessType = AccessType.Premium, CategoryName = "Инструменти", ReviewCount = 0, AverageRating = 0, IsFavorite = false, ViewsCount = 0 }
            };

            _testUser = new ApplicationUser { Id = "testUserId", UserName = "testuser" };
            _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUser.Id),
                new Claim(ClaimTypes.Name, _testUser.UserName)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _userPrincipal }
            };
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [Fact]
        public async Task All_ShouldReturnViewWithServices()
        {
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());
            _mockServiceService.Setup(s => s.GetAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                               .ReturnsAsync(_serviceViewModels);

            var result = await _controller.All(null, null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            _mockCategoryRepository.Verify(repo => repo.All(), Times.Once);
            _mockServiceService.Verify(s => s.GetAllAsync(null, null, null, null, _testUser.Id), Times.Once);
        }

        [Fact]
        public async Task All_ShouldApplyCategoryFilter()
        {
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());
            _mockServiceService.Setup(s => s.GetAllAsync("Документи", null, null, null, It.IsAny<string>()))
                               .ReturnsAsync(_serviceViewModels.Where(s => s.CategoryName == "Документи"));

            var result = await _controller.All("Документи", null, null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ServiceViewModel>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Service X", model.First().Title);
            _mockServiceService.Verify(s => s.GetAllAsync("Документи", null, null, null, _testUser.Id), Times.Once);
        }

        [Fact]
        public async Task Details_ShouldReturnViewWithServiceDetails_AndIncrementViews()
        {
            var serviceId = _serviceViewModels[0].Id;
            var service = _serviceViewModels[0];
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<string>())).ReturnsAsync(service);
            _mockServiceService.Setup(s => s.IncrementViewsCount(serviceId)).Returns(Task.CompletedTask);
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "Admin")).ReturnsAsync(false);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "BusinessUser")).ReturnsAsync(false);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "User")).ReturnsAsync(true);

            var result = await _controller.Details(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceViewModel>(viewResult.Model);
            Assert.Equal(serviceId, model.Id);
            Assert.Equal("Service X", model.Title);
            Assert.True((bool)viewResult.ViewData["CanUseService"]);

            _mockServiceService.Verify(s => s.IncrementViewsCount(serviceId), Times.Once);
            _mockServiceService.Verify(s => s.GetByIdAsync(serviceId, _testUser.Id), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Controller.Details: Извикан IncrementViewsCount за ServiceId: {serviceId}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Details_ShouldReturnNotFound_WhenServiceDoesNotExist()
        {
            var nonExistentServiceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.GetByIdAsync(nonExistentServiceId, It.IsAny<string>())).ReturnsAsync((ServiceViewModel)null);
            _mockServiceService.Setup(s => s.IncrementViewsCount(nonExistentServiceId)).Returns(Task.CompletedTask);

            var result = await _controller.Details(nonExistentServiceId);

            Assert.IsType<NotFoundResult>(result);
            _mockServiceService.Verify(s => s.IncrementViewsCount(nonExistentServiceId), Times.Once);
            _mockServiceService.Verify(s => s.GetByIdAsync(nonExistentServiceId, _testUser.Id), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Услуга с ID {nonExistentServiceId} не е намерена след увеличение на брояча.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Details_ShouldSetCanUseServiceToTrue_ForAdminUser()
        {
            var serviceId = _serviceViewModels[1].Id;
            var service = _serviceViewModels[1];
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<string>())).ReturnsAsync(service);
            _mockServiceService.Setup(s => s.IncrementViewsCount(serviceId)).Returns(Task.CompletedTask);

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "Admin")).ReturnsAsync(true);

            var result = await _controller.Details(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True((bool)viewResult.ViewData["CanUseService"]);
        }

        [Fact]
        public async Task Details_ShouldSetCanUseServiceToFalse_ForRegularUserOnPremiumService()
        {
            var serviceId = _serviceViewModels[1].Id;
            var service = _serviceViewModels[1];
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<string>())).ReturnsAsync(service);
            _mockServiceService.Setup(s => s.IncrementViewsCount(serviceId)).Returns(Task.CompletedTask);

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "Admin")).ReturnsAsync(false);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "BusinessUser")).ReturnsAsync(false);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "User")).ReturnsAsync(true);

            var result = await _controller.Details(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False((bool)viewResult.ViewData["CanUseService"]);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldAddFavorite_WhenNotAlreadyFavorite()
        {
            var serviceId = Guid.NewGuid();
            var userId = _testUser.Id;
            _mockFavoriteRepo.Setup(repo => repo.All()).Returns(new List<Favorite>().AsQueryable());
            _mockFavoriteRepo.Setup(repo => repo.AddAsync(It.IsAny<Favorite>())).Returns(Task.CompletedTask);
            _mockFavoriteRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _controller.ToggleFavorite(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ServiceController.Details), redirectToActionResult.ActionName);
            Assert.Equal(serviceId, redirectToActionResult.RouteValues["id"]);
            Assert.Equal("Added to favorites!", _controller.TempData["SuccessMessage"]);

            _mockFavoriteRepo.Verify(repo => repo.AddAsync(It.Is<Favorite>(f => f.ServiceId == serviceId && f.UserId == userId)), Times.Once);
            _mockFavoriteRepo.Verify(repo => repo.Delete(It.IsAny<Favorite>()), Times.Never);
            _mockFavoriteRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldRemoveFavorite_WhenAlreadyFavorite()
        {
            var serviceId = Guid.NewGuid();
            var userId = _testUser.Id;
            var existingFavorite = new Favorite { ServiceId = serviceId, UserId = userId, CreatedOn = DateTime.UtcNow };
            _mockFavoriteRepo.Setup(repo => repo.All()).Returns(new List<Favorite> { existingFavorite }.AsQueryable());
            _mockFavoriteRepo.Setup(repo => repo.Delete(It.IsAny<Favorite>()));
            _mockFavoriteRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _controller.ToggleFavorite(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ServiceController.Details), redirectToActionResult.ActionName);
            Assert.Equal(serviceId, redirectToActionResult.RouteValues["id"]);
            Assert.Equal("Removed from favorites!", _controller.TempData["SuccessMessage"]);

            _mockFavoriteRepo.Verify(repo => repo.AddAsync(It.IsAny<Favorite>()), Times.Never);
            _mockFavoriteRepo.Verify(repo => repo.Delete(It.Is<Favorite>(f => f.ServiceId == serviceId && f.UserId == userId)), Times.Once);
            _mockFavoriteRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
            var serviceId = Guid.NewGuid();

            var result = await _controller.ToggleFavorite(serviceId);

            Assert.IsType<UnauthorizedResult>(result);
            _mockFavoriteRepo.Verify(repo => repo.All(), Times.Never);
            _mockFavoriteRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UseService_ShouldReturnNotFound_WhenServiceDoesNotExist()
        {
            _mockServiceRepository.Setup(repo => repo.All()).Returns(new List<Service>().AsQueryable());

            var result = await _controller.UseService(Guid.NewGuid());

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Shared/Error.cshtml", viewResult.ViewName);
            Assert.Equal("Услугата не е намерена.", _controller.TempData["ErrorMessage"]);
            _mockServiceRepository.Verify(repo => repo.All(), Times.Once);
        }

        [Fact]
        public async Task UseService_ShouldRedirectToLogin_WhenUserNotAuthenticated()
        {
            var serviceId = ServiceConstants.FileConverterServiceId;
            var service = new Service { Id = serviceId, Title = "File Converter", AccessType = AccessType.Free };
            _mockServiceRepository.Setup(repo => repo.All()).Returns(new List<Service> { service }.AsQueryable());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            var result = await _controller.UseService(serviceId);

            var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Account/Login", redirectToPageResult.PageName);
            Assert.Equal("Identity", redirectToPageResult.RouteValues["area"]);
            Assert.Equal("Моля, влезте в профила си, за да използвате услугата.", _controller.TempData["ErrorMessage"]);
        }

        
        [Fact]
        public async Task UseService_ShouldReturnCorrectView_ForFreeServiceAndRegularUser()
        {
            var serviceId = ServiceConstants.FileConverterServiceId;
            var service = new Service { Id = serviceId, Title = "File Converter", AccessType = AccessType.Free };
            _mockServiceRepository.Setup(repo => repo.All()).Returns(new List<Service> { service }.AsQueryable());

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "Admin")).ReturnsAsync(false);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "BusinessUser")).ReturnsAsync(false);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "User")).ReturnsAsync(true);

            var result = await _controller.UseService(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_FileConverterForm.cshtml", viewResult.ViewName);
            Assert.Equal("Вие използвате услугата: File Converter (Ограничен достъп).", _controller.TempData["ServiceMessage"]);
            Assert.NotNull(viewResult.ViewData["SupportedFormats"]);
        }

        [Fact]
        public async Task UseService_ShouldReturnCorrectView_ForAdminUserOnAnyService()
        {
            var serviceId = ServiceConstants.AutoCvResumeServiceId;
            var service = new Service { Id = serviceId, Title = "Auto CV/Resume", AccessType = AccessType.Premium };
            _mockServiceRepository.Setup(repo => repo.All()).Returns(new List<Service> { service }.AsQueryable());

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
            _mockUserManager.Setup(um => um.IsInRoleAsync(_testUser, "Admin")).ReturnsAsync(true);

            var result = await _controller.UseService(serviceId);

            var redirectToResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/CvGenerator/CvGeneratorForm", redirectToResult.Url);
            Assert.Equal("Вие използвате услугата: Auto CV/Resume (Пълен достъп).", _controller.TempData["ServiceMessage"]);
        }

      
    }
}
