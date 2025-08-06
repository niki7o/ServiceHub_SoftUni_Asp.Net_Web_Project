using Xunit;
using Moq;
using ServiceHub.Controllers;
using ServiceHub.Services.Interfaces;
using ServiceHub.Core.Models.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceHub.Data.Models;
using Microsoft.AspNetCore.Identity;
using ServiceHub.Common.Enum;
using ServiceHub.Common;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Primitives;
using System.Text;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;

        private readonly ServiceController _controller;

        private readonly List<ServiceViewModel> _serviceViewModels;
        private readonly List<Category> _categories;
        private readonly ApplicationUser _testUser;

        public ServiceControllerTests()
        {
            _mockLogger = new Mock<ILogger<ServiceController>>();
            _mockServiceDispatcher = new Mock<IServiceDispatcher>();
            _mockServiceService = new Mock<IServiceService>();
            _mockFavoriteRepo = new Mock<IRepository<Favorite>>();
            _mockServiceRepository = new Mock<IRepository<Service>>();
            _mockCategoryRepository = new Mock<IRepository<Category>>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            _controller = new ServiceController(
                _mockServiceDispatcher.Object,
                _mockServiceService.Object,
                _mockUserManager.Object,
                _mockFavoriteRepo.Object,
                _mockServiceRepository.Object,
                _mockCategoryRepository.Object,
                _mockLogger.Object,
                _mockServiceScopeFactory.Object
            );

            _categories = new List<Category>
            {
                new Category { Id = Guid.Parse("A0A0A0A0-A0A0-A0A0-A0A0-000000000001"), Name = "Документи", Description = "Документи", CreatedOn = DateTime.UtcNow },
                new Category { Id = Guid.Parse("B1B1B1B1-B1B1-B1B1-B1B1-000000000002"), Name = "Инструменти", Description = "Инструменти", CreatedOn = DateTime.UtcNow }
            };

            _serviceViewModels = new List<ServiceViewModel>
            {
                new ServiceViewModel { Id = Guid.NewGuid(), Title = "Service X", Description = "Desc X", AccessType = AccessType.Free, CategoryName = "Документи", ReviewCount = 0, AverageRating = 0, IsFavorite = false, ViewsCount = 0, IsApproved = true, IsTemplate = false },
                new ServiceViewModel { Id = Guid.NewGuid(), Title = "Service Y", Description = "Desc Y", AccessType = AccessType.Premium, CategoryName = "Инструменти", ReviewCount = 0, AverageRating = 0, IsFavorite = false, ViewsCount = 0, IsApproved = true, IsTemplate = false }
            };

            _testUser = new ApplicationUser { Id = "testUserId", UserName = "testuser" };

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceService))).Returns(_mockServiceService.Object);
            var mockServiceScope = new Mock<IServiceScope>();
            mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            _mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);
        }

        private void SetupUserContext(ApplicationUser user, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin"))
                            .ReturnsAsync(roles.Contains("Admin"));
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "BusinessUser"))
                            .ReturnsAsync(roles.Contains("BusinessUser"));
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "User"))
                            .ReturnsAsync(roles.Contains("User"));
        }

        private void SetupEmptyUserContext()
        {
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

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
        public async Task Details_ShouldReturnRedirectToAll_WhenServiceDoesNotExist()
        {
            SetupUserContext(_testUser, "User");
            var nonExistentServiceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.GetByIdAsync(nonExistentServiceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new ArgumentException("Услугата не е намерена."));

            var result = await _controller.Details(nonExistentServiceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ServiceController.All), redirectToActionResult.ActionName);
            _controller.TempData.TryGetValue("ErrorMessage", out var errorMessage);
            Assert.Equal("Услугата не е намерена.", errorMessage);

            _mockServiceService.Verify(s => s.IncrementViewsCount(It.IsAny<Guid>()), Times.Never);
            _mockServiceService.Verify(s => s.GetByIdAsync(nonExistentServiceId, _testUser.Id, 1, 2), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Details Action: ArgumentException for Service ID: {nonExistentServiceId}, Review Page: 1. Message: Услугата не е намерена.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Details_ShouldSetCanUseServiceToTrue_ForAdminUser()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = _serviceViewModels[1].Id;
            var service = _serviceViewModels[1];

            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);
            _mockServiceService.Setup(s => s.IncrementViewsCount(serviceId)).Returns(Task.CompletedTask);

            var result = await _controller.Details(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceViewModel>(viewResult.Model);
            Assert.Equal(serviceId, model.Id);
        }

        [Fact]
        public async Task Details_ShouldSetCanUseServiceToFalse_ForRegularUserOnPremiumService()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = _serviceViewModels[1].Id;
            var service = _serviceViewModels[1];

            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);
            _mockServiceService.Setup(s => s.IncrementViewsCount(serviceId)).Returns(Task.CompletedTask);

            var result = await _controller.Details(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ServiceViewModel>(viewResult.Model);
            Assert.Equal(serviceId, model.Id);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldAddFavorite_WhenNotAlreadyFavorite()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            var userId = _testUser.Id;

            _mockServiceService.Setup(s => s.ToggleFavorite(serviceId, userId)).Returns(Task.CompletedTask);

            var result = await _controller.ToggleFavorite(serviceId, null, null, null, null);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Статусът на любими е променен успешно!", _controller.TempData["SuccessMessage"]);

            _mockServiceService.Verify(s => s.ToggleFavorite(serviceId, userId), Times.Once);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldRemoveFavorite_WhenAlreadyFavorite()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            var userId = _testUser.Id;

            _mockServiceService.Setup(s => s.ToggleFavorite(serviceId, userId)).Returns(Task.CompletedTask);

            var result = await _controller.ToggleFavorite(serviceId, null, null, null, null);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Статусът на любими е променен успешно!", _controller.TempData["SuccessMessage"]);

            _mockServiceService.Verify(s => s.ToggleFavorite(serviceId, userId), Times.Once);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            SetupEmptyUserContext();
            var serviceId = Guid.NewGuid();

            var result = await _controller.ToggleFavorite(serviceId, null, null, null, null);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UseService_ShouldReturnNotFound_WhenServiceDoesNotExist()
        {
            SetupUserContext(_testUser, "User");
            var nonExistentServiceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.GetByIdAsync(nonExistentServiceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new ArgumentException("Услугата не е намерена."));

            var result = await _controller.UseService(nonExistentServiceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Shared/Error.cshtml", viewResult.ViewName);
            Assert.Equal("Услугата не е намерена.", _controller.TempData["ErrorMessage"]);
        }

        

        [Fact]
        public async Task UseService_ShouldReturnCorrectView_ForFreeServiceAndRegularUser()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = ServiceConstants.FileConverterServiceId;
            var service = new ServiceViewModel { Id = serviceId, Title = "File Converter", AccessType = AccessType.Free, IsApproved = true, IsTemplate = false };
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);

            var result = await _controller.UseService(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_FileConverterForm.cshtml", viewResult.ViewName);
            Assert.Equal("Вие използвате услугата: File Converter (Ограничен достъп).", _controller.TempData["ServiceMessage"]);
            Assert.NotNull(viewResult.ViewData["SupportedFormats"]);
        }

        [Fact]
        public async Task UseService_ShouldReturnCorrectView_ForAdminUserOnAnyService()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = ServiceConstants.AutoCvResumeServiceId;
            var service = new ServiceViewModel { Id = serviceId, Title = "Auto CV/Resume", AccessType = AccessType.Premium, IsApproved = true, IsTemplate = false };
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);

            var result = await _controller.UseService(serviceId);

            var redirectToResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal("/CvGenerator/CvGeneratorForm", redirectToResult.Url);
            Assert.Equal("Вие използвате услугата: Auto CV/Resume (Пълен достъп).", _controller.TempData["ServiceMessage"]);
        }

        [Fact]
        public async Task UseService_ShouldRedirectToDetails_WhenServiceIsUnapprovedTemplate()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            var service = new ServiceViewModel { Id = serviceId, Title = "Unapproved Template", IsTemplate = true, IsApproved = false };
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(service);

            var result = await _controller.UseService(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectToActionResult.ActionName);
            Assert.Equal("Service", redirectToActionResult.ControllerName);
            Assert.Equal(serviceId, redirectToActionResult.RouteValues["id"]);
            Assert.Equal("Тази услуга е шаблон и все още не е одобрена за използване.", _controller.TempData["ErrorMessage"]);
        }

        
        [Fact]
        public async Task ExecuteService_ShouldReturnBadRequest_WhenServiceIdIsInvalid()
        {
            SetupEmptyUserContext();
            var form = new FormCollection(new Dictionary<string, StringValues> { { "serviceId", "" } });

            var result = await _controller.ExecuteService(form);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var jsonResult = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Невалиден или липсващ идентификатор на услугата.".Trim(), jsonResult["message"].ToString().Trim());
        }

        [Fact]
        public async Task ExecuteService_ShouldReturnBadRequest_WhenFileIsMissingForFileConverter()
        {
            SetupUserContext(_testUser, "User");
            var form = new FormCollection(new Dictionary<string, StringValues> { { "serviceId", ServiceConstants.FileConverterServiceId.ToString() } });

            var result = await _controller.ExecuteService(form);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var jsonResult = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Файлът е задължителен за услугата за конвертиране на файлове.".Trim(), jsonResult["message"].ToString().Trim());
        }

       

        [Fact]
        public async Task ToggleFavorite_ShouldRedirectToAll_OnSuccess()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.ToggleFavorite(serviceId, _testUser.Id)).Returns(Task.CompletedTask);

            var result = await _controller.ToggleFavorite(serviceId, null, null, null, null);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Статусът на любими е променен успешно!", _controller.TempData["SuccessMessage"]);
            _mockServiceService.Verify(s => s.ToggleFavorite(serviceId, _testUser.Id), Times.Once);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldHandleArgumentException()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.ToggleFavorite(serviceId, _testUser.Id)).ThrowsAsync(new ArgumentException("Service not found."));

            var result = await _controller.ToggleFavorite(serviceId, null, null, null, null);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Service not found.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldHandleInvalidOperationException()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.ToggleFavorite(serviceId, _testUser.Id)).ThrowsAsync(new InvalidOperationException("Не може да добавяте неодобрени шаблони към любими."));

            var result = await _controller.ToggleFavorite(serviceId, null, null, null, null);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Не може да добавяте неодобрени шаблони към любими.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Details_ShouldRedirectToAll_OnArgumentException()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>()))
                               .ThrowsAsync(new ArgumentException("Service not found."));

            var result = await _controller.Details(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ServiceController.All), redirectToActionResult.ActionName);
            Assert.Equal("Service not found.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Details_ShouldRedirectToAll_OnUnauthorizedAccessException()
        {
            SetupUserContext(_testUser, "User");
            var serviceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.GetByIdAsync(serviceId, _testUser.Id, It.IsAny<int>(), It.IsAny<int>()))
                               .ThrowsAsync(new UnauthorizedAccessException("You are not authorized to view this unapproved template."));

            var result = await _controller.Details(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ServiceController.All), redirectToActionResult.ActionName);
            Assert.Equal("You are not authorized to view this unapproved template.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Create_ShouldReturnViewWithCategories()
        {
            SetupUserContext(_testUser, "Admin");
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ServiceFormModel>(viewResult.Model);
            Assert.NotNull(model.Categories);
            Assert.Equal(2, model.Categories.Count());
        }

        [Fact]
        public async Task Create_ShouldRedirectToAll_OnSuccess()
        {
            SetupUserContext(_testUser, "Admin");
            var model = new ServiceFormModel { Title = "New Service", Description = "Desc", CategoryId = _categories[0].Id, AccessType = AccessType.Free };
            _mockServiceService.Setup(s => s.CreateAsync(model, _testUser.Id)).Returns(Task.CompletedTask);

            var result = await _controller.Create(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Услугата е създадена успешно!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Create_ShouldReturnView_OnInvalidModelState()
        {
            SetupUserContext(_testUser, "Admin");
            var model = new ServiceFormModel { Title = "", Description = "Desc", CategoryId = _categories[0].Id, AccessType = AccessType.Free };
            _controller.ModelState.AddModelError("Title", "Title is required");
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ServiceFormModel>(viewResult.Model);
            Assert.Equal("Невалидни данни за услугата. Моля, проверете въведеното.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task CreateTemplate_ShouldReturnViewWithCategories()
        {
            SetupUserContext(_testUser, "BusinessUser");
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());

            var result = await _controller.CreateTemplate();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ServiceFormModel>(viewResult.Model);
            Assert.NotNull(model.Categories);
            Assert.Equal(2, model.Categories.Count());
        }

        [Fact]
        public async Task AddTemplate_ShouldRedirectToAll_OnSuccessForAdmin()
        {
            SetupUserContext(_testUser, "Admin");
            var model = new ServiceFormModel { Title = "New Template", Description = "Desc", CategoryId = _categories[0].Id, AccessType = AccessType.Free };
            _mockServiceService.Setup(s => s.AddServiceTemplateAsync(model, _testUser.Id, true)).Returns(Task.CompletedTask);

            var result = await _controller.AddTemplate(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Услугата е създадена успешно (директно одобрена като администратор)!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task AddTemplate_ShouldRedirectToAll_OnSuccessForBusinessUser()
        {
            SetupUserContext(_testUser, "BusinessUser");
            var model = new ServiceFormModel { Title = "New Template", Description = "Desc", CategoryId = _categories[0].Id, AccessType = AccessType.Free };
            _mockServiceService.Setup(s => s.AddServiceTemplateAsync(model, _testUser.Id, false)).Returns(Task.CompletedTask);

            var result = await _controller.AddTemplate(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Шаблонът за услуга е изпратен за одобрение!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task AddTemplate_ShouldReturnView_OnInvalidModelState()
        {
            SetupUserContext(_testUser, "BusinessUser");
            var model = new ServiceFormModel { Title = "", Description = "Desc", CategoryId = _categories[0].Id, AccessType = AccessType.Free };
            _controller.ModelState.AddModelError("Title", "Title is required");
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());

            var result = await _controller.AddTemplate(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateTemplate", viewResult.ViewName);
            Assert.IsType<ServiceFormModel>(viewResult.Model);
            Assert.Equal("Невалидни данни за шаблона. Моля, проверете въведеното.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Edit_Get_ShouldReturnViewWithModel()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = _serviceViewModels[0].Id;
            var serviceFormModel = new ServiceFormModel { Title = "Service X", Description = "Desc X", CategoryId = _categories[0].Id, AccessType = AccessType.Free };
            _mockServiceService.Setup(s => s.GetServiceForEditAsync(serviceId, _testUser.Id, true)).ReturnsAsync(serviceFormModel);
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());

            var result = await _controller.Edit(serviceId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ServiceFormModel>(viewResult.Model);
            Assert.Equal(serviceFormModel.Title, model.Title);
            Assert.NotNull(model.Categories);
        }

        [Fact]
        public async Task Edit_Get_ShouldRedirectToAll_OnArgumentException()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.GetServiceForEditAsync(serviceId, _testUser.Id, true)).ThrowsAsync(new ArgumentException("Service not found."));

            var result = await _controller.Edit(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Service not found.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Edit_Post_ShouldRedirectToDetails_OnSuccess()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = _serviceViewModels[0].Id;
            var model = new ServiceFormModel { Title = "Updated Title", Description = "Updated Desc", CategoryId = _categories[1].Id, AccessType = AccessType.Premium };
            _mockServiceService.Setup(s => s.UpdateAsync(serviceId, model, _testUser.Id, true)).Returns(Task.CompletedTask);

            var result = await _controller.Edit(serviceId, model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectToActionResult.ActionName);
            Assert.Equal(serviceId, redirectToActionResult.RouteValues["id"]);
            Assert.Equal("Услугата е редактирана успешно!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Edit_Post_ShouldReturnView_OnInvalidModelState()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = _serviceViewModels[0].Id;
            var model = new ServiceFormModel { Title = "", Description = "Desc", CategoryId = _categories[0].Id, AccessType = AccessType.Free };
            _controller.ModelState.AddModelError("Title", "Title is required");
            _mockCategoryRepository.Setup(repo => repo.All()).Returns(_categories.AsQueryable());

            var result = await _controller.Edit(serviceId, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ServiceFormModel>(viewResult.Model);
            Assert.Equal("Невалидни данни за услугата. Моля, проверете въведеното.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Delete_ShouldRedirectToAll_OnSuccess()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = _serviceViewModels[0].Id;
            _mockServiceService.Setup(s => s.DeleteAsync(serviceId, _testUser.Id, true)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Услугата е изтрита успешно!", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Delete_ShouldHandleArgumentException()
        {
            SetupUserContext(_testUser, "Admin");
            var serviceId = Guid.NewGuid();
            _mockServiceService.Setup(s => s.DeleteAsync(serviceId, _testUser.Id, true)).ThrowsAsync(new ArgumentException("Service not found."));

            var result = await _controller.Delete(serviceId);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("All", redirectToActionResult.ActionName);
            Assert.Equal("Service not found.", _controller.TempData["ErrorMessage"]);
        }
    }
}
