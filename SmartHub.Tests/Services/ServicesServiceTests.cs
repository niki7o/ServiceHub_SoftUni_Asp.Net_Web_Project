using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Common;
using ServiceHub.Common.Enum;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using ServiceHub.Services.Services;
using ServiceHub.Tests; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.Services
{
    public class NonFileConvertRequest : BaseServiceRequest
    {
    }

    public class ServicesServiceTests
    {
        private readonly Mock<IRepository<Service>> _mockServiceRepo;
        private readonly Mock<IRepository<Category>> _mockCategoryRepo;
        private readonly Mock<IRepository<Review>> _mockReviewRepo;
        private readonly Mock<IRepository<Favorite>> _mockFavoriteRepo;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<ServicesService>> _mockLogger;
        private readonly ServicesService _servicesService;

        private readonly List<Service> _services;
        private readonly List<Category> _categories;
        private readonly List<ApplicationUser> _users;
        private readonly List<Favorite> _favorites;
        private readonly List<Review> _reviews;

        public ServicesServiceTests()
        {
            _mockServiceRepo = new Mock<IRepository<Service>>();
            _mockCategoryRepo = new Mock<IRepository<Category>>();
            _mockReviewRepo = new Mock<IRepository<Review>>();
            _mockFavoriteRepo = new Mock<IRepository<Favorite>>();
            _mockLogger = new Mock<ILogger<ServicesService>>();

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            _servicesService = new ServicesService(
                _mockServiceRepo.Object,
                _mockCategoryRepo.Object,
                _mockReviewRepo.Object,
                _mockFavoriteRepo.Object,
                _mockUserManager.Object,
                _mockLogger.Object
            );

            _categories = new List<Category>
            {
                new Category { Id = Guid.Parse("A0A0A0A0-A0A0-A0A0-A0A0-000000000001"), Name = "Документи", Description = "Документи", CreatedOn = DateTime.UtcNow },
                new Category { Id = Guid.Parse("B1B1B1B1-B1B1-B1B1-B1B1-000000000002"), Name = "Инструменти", Description = "Инструменти", CreatedOn = DateTime.UtcNow }
            };

            _users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "testUserId1", UserName = "user1", Email = "user1@test.com" },
                new ApplicationUser { Id = "testUserId2", UserName = "user2", Email = "user2@test.com" }
            };

            _services = new List<Service>
            {
                new Service
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Service A",
                    Description = "Desc A",
                    AccessType = AccessType.Free,
                    CategoryId = _categories[0].Id,
                    Category = _categories[0],
                    CreatedOn = DateTime.UtcNow.AddDays(-5),
                    ViewsCount = 10
                },
                new Service
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Title = "Service B",
                    Description = "Desc B",
                    AccessType = AccessType.Partial,
                    CategoryId = _categories[1].Id,
                    Category = _categories[1],
                    CreatedOn = DateTime.UtcNow.AddDays(-10),
                    ViewsCount = 20
                },
                new Service
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Title = "Service C",
                    Description = "Desc C",
                    AccessType = AccessType.Premium,
                    CategoryId = _categories[0].Id,
                    Category = _categories[0],
                    CreatedOn = DateTime.UtcNow.AddDays(-1),
                    ViewsCount = 5
                }
            };

            _reviews = new List<Review>
            {
                new Review { Id = Guid.NewGuid(), ServiceId = _services[0].Id, UserId = _users[0].Id, Rating = 5, Comment = "Great!", CreatedOn = DateTime.UtcNow, User = _users[0] },
                new Review { Id = Guid.NewGuid(), ServiceId = _services[0].Id, UserId = _users[1].Id, Rating = 4, Comment = "Good!", CreatedOn = DateTime.UtcNow, User = _users[1] }
            };
            _services[0].Reviews = _reviews.Where(r => r.ServiceId == _services[0].Id).ToList();

            _favorites = new List<Favorite>
            {
                new Favorite { ServiceId = _services[0].Id, UserId = _users[0].Id, CreatedOn = DateTime.UtcNow }
            };
            _services[0].Favorites = _favorites.Where(f => f.ServiceId == _services[0].Id).ToList();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllServices_WhenNoFiltersApplied()
        {
            _mockServiceRepo.Setup(repo => repo.All()).Returns(_services.AsQueryable());

            var result = await _servicesService.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.Contains(result, s => s.Title == "Service A");
            Assert.Contains(result, s => s.Title == "Service B");
            Assert.Contains(result, s => s.Title == "Service C");
            _mockServiceRepo.Verify(repo => repo.All(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByCategory()
        {
            _mockServiceRepo.Setup(repo => repo.All()).Returns(_services.AsQueryable());

            var result = await _servicesService.GetAllAsync(categoryFilter: "Документи");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, s => s.Title == "Service A");
            Assert.Contains(result, s => s.Title == "Service C");
            Assert.DoesNotContain(result, s => s.Title == "Service B");
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByAccessType()
        {
            _mockServiceRepo.Setup(repo => repo.All()).Returns(_services.AsQueryable());

            var result = await _servicesService.GetAllAsync(accessTypeFilter: "Free");

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, s => s.Title == "Service A");
        }

        

        [Fact]
        public async Task GetAllAsync_ShouldSortByTitleAscending()
        {
            _mockServiceRepo.Setup(repo => repo.All()).Returns(_services.AsQueryable());

            var result = await _servicesService.GetAllAsync(sort: "az");

            Assert.NotNull(result);
            var titles = result.Select(s => s.Title).ToList();
            Assert.Equal(new List<string> { "Service A", "Service B", "Service C" }, titles);
        }

        [Fact]
        public async Task GetAllAsync_ShouldSortByViewsCountDescending()
        {
            _mockServiceRepo.Setup(repo => repo.All()).Returns(_services.AsQueryable());

            var result = await _servicesService.GetAllAsync(sort: "mostviewed");

            Assert.NotNull(result);
            var titles = result.Select(s => s.Title).ToList();
            Assert.Equal(new List<string> { "Service B", "Service A", "Service C" }, titles);
        }

       

        [Fact]
        public async Task ToggleFavorite_ShouldAddFavorite_WhenNotAlreadyFavorite()
        {
            var serviceId = _services[1].Id;
            var userId = _users[0].Id;

            _mockFavoriteRepo.Setup(repo => repo.All()).Returns(new List<Favorite>().AsQueryable());
            _mockFavoriteRepo.Setup(repo => repo.AddAsync(It.IsAny<Favorite>())).Returns(Task.CompletedTask);
            _mockFavoriteRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.ToggleFavorite(serviceId, userId);

            _mockFavoriteRepo.Verify(repo => repo.AddAsync(It.Is<Favorite>(f => f.ServiceId == serviceId && f.UserId == userId)), Times.Once);
            _mockFavoriteRepo.Verify(repo => repo.Delete(It.IsAny<Favorite>()), Times.Never);
            _mockFavoriteRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldRemoveFavorite_WhenAlreadyFavorite()
        {
            var serviceId = _services[0].Id;
            var userId = _users[0].Id;

            _mockFavoriteRepo.Setup(repo => repo.All()).Returns(_favorites.AsQueryable());
            _mockFavoriteRepo.Setup(repo => repo.Delete(It.IsAny<Favorite>()));
            _mockFavoriteRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.ToggleFavorite(serviceId, userId);

            _mockFavoriteRepo.Verify(repo => repo.AddAsync(It.IsAny<Favorite>()), Times.Never);
            _mockFavoriteRepo.Verify(repo => repo.Delete(It.Is<Favorite>(f => f.ServiceId == serviceId && f.UserId == userId)), Times.Once);
            _mockFavoriteRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

      

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            _mockServiceRepo.Setup(repo => repo.All()).Returns(new List<Service>().AsQueryable());

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.GetByIdAsync(nonExistentServiceId));
            Assert.Equal("Service not found.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.All(), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Услуга с ID {nonExistentServiceId} не е намерена. Хвърля се ArgumentException.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldAddServiceAndSaveChanges()
        {
            var newServiceFormModel = new ServiceFormModel
            {
                Title = "New Service",
                Description = "New Description",
                CategoryId = _categories[0].Id,
                AccessType = AccessType.Free
            };

            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(newServiceFormModel.CategoryId)).ReturnsAsync(_categories[0]);
            _mockServiceRepo.Setup(repo => repo.AddAsync(It.IsAny<Service>())).Returns(Task.CompletedTask);
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.CreateAsync(newServiceFormModel);

            _mockServiceRepo.Verify(repo => repo.AddAsync(It.Is<Service>(s => s.Title == newServiceFormModel.Title && s.Description == newServiceFormModel.Description)), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowArgumentException_WhenCategoryNotFound()
        {
            var newServiceFormModel = new ServiceFormModel
            {
                Title = "New Service",
                Description = "New Description",
                CategoryId = Guid.NewGuid(),
                AccessType = AccessType.Free
            };

            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(newServiceFormModel.CategoryId)).ReturnsAsync((Category)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.CreateAsync(newServiceFormModel));
            Assert.Equal("Invalid category selected.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.AddAsync(It.IsAny<Service>()), Times.Never);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateServiceAndSaveChanges()
        {
            var serviceToUpdateId = _services[0].Id;
            var updatedServiceFormModel = new ServiceFormModel
            {
                Title = "Updated Service A",
                Description = "Updated Desc A",
                CategoryId = _categories[1].Id,
                AccessType = AccessType.Premium
            };

            var existingService = _services[0];
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToUpdateId)).ReturnsAsync(existingService);
            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(updatedServiceFormModel.CategoryId)).ReturnsAsync(_categories[1]);
            _mockServiceRepo.Setup(repo => repo.Update(It.IsAny<Service>()));
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.UpdateAsync(serviceToUpdateId, updatedServiceFormModel);

            Assert.Equal(updatedServiceFormModel.Title, existingService.Title);
            Assert.Equal(updatedServiceFormModel.Description, existingService.Description);
            Assert.Equal(updatedServiceFormModel.CategoryId, existingService.CategoryId);
            Assert.Equal(updatedServiceFormModel.AccessType, existingService.AccessType);
            Assert.NotNull(existingService.ModifiedOn);
            _mockServiceRepo.Verify(repo => repo.Update(existingService), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            var model = new ServiceFormModel();
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.UpdateAsync(nonExistentServiceId, model));
            Assert.Equal("Service not found.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.Update(It.IsAny<Service>()), Times.Never);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenCategoryNotFound()
        {
            var serviceToUpdateId = _services[0].Id;
            var updatedServiceFormModel = new ServiceFormModel
            {
                Title = "Updated Service A",
                Description = "Updated Desc A",
                CategoryId = Guid.NewGuid(),
                AccessType = AccessType.Premium
            };

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToUpdateId)).ReturnsAsync(_services[0]);
            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(updatedServiceFormModel.CategoryId)).ReturnsAsync((Category)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.UpdateAsync(serviceToUpdateId, updatedServiceFormModel));
            Assert.Equal("Invalid category selected.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.Update(It.IsAny<Service>()), Times.Never);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteServiceAndSaveChanges()
        {
            var serviceToDeleteId = _services[0].Id;
            var serviceToDelete = _services[0];
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToDeleteId)).ReturnsAsync(serviceToDelete);
            _mockServiceRepo.Setup(repo => repo.Delete(It.IsAny<Service>()));
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.DeleteAsync(serviceToDeleteId);

            _mockServiceRepo.Verify(repo => repo.Delete(serviceToDelete), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.DeleteAsync(nonExistentServiceId));
            Assert.Equal("Service not found.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.Delete(It.IsAny<Service>()), Times.Never);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AddReviewAsync_ShouldAddReviewAndSaveChanges()
        {
            var serviceId = _services[0].Id;
            var userId = _users[0].Id;
            var reviewModel = new ReviewFormModel { Rating = 4, Comment = "New comment" };

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(_services[0]);
            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(_users[0]);
            _mockReviewRepo.Setup(repo => repo.AddAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);
            _mockReviewRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.AddReviewAsync(serviceId, userId, reviewModel);

            _mockReviewRepo.Verify(repo => repo.AddAsync(It.Is<Review>(r => r.ServiceId == serviceId && r.UserId == userId && r.Rating == reviewModel.Rating && r.Comment == reviewModel.Comment)), Times.Once);
            _mockReviewRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddReviewAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            var userId = _users[0].Id;
            var reviewModel = new ReviewFormModel { Rating = 4, Comment = "New comment" };

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.AddReviewAsync(nonExistentServiceId, userId, reviewModel));
            Assert.Equal("Service not found.", exception.Message);
            _mockReviewRepo.Verify(repo => repo.AddAsync(It.IsAny<Review>()), Times.Never);
            _mockReviewRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AddReviewAsync_ShouldThrowArgumentException_WhenUserNotFound()
        {
            var serviceId = _services[0].Id;
            var nonExistentUserId = "nonExistentUser";
            var reviewModel = new ReviewFormModel { Rating = 4, Comment = "New comment" };

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(_services[0]);
            _mockUserManager.Setup(um => um.FindByIdAsync(nonExistentUserId)).ReturnsAsync((ApplicationUser)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.AddReviewAsync(serviceId, nonExistentUserId, reviewModel));
            Assert.Equal("User not found.", exception.Message);
            _mockReviewRepo.Verify(repo => repo.AddAsync(It.IsAny<Review>()), Times.Never);
            _mockReviewRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetServiceForEditAsync_ShouldReturnServiceFormModel_WhenServiceExists()
        {
            var serviceId = _services[0].Id;
            var serviceToEdit = _services[0];
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(serviceToEdit);

            var result = await _servicesService.GetServiceForEditAsync(serviceId);

            Assert.NotNull(result);
            Assert.Equal(serviceToEdit.Title, result.Title);
            Assert.Equal(serviceToEdit.Description, result.Description);
            Assert.Equal(serviceToEdit.CategoryId, result.CategoryId);
            Assert.Equal(serviceToEdit.AccessType, result.AccessType);
        }

        [Fact]
        public async Task GetServiceForEditAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.GetServiceForEditAsync(nonExistentServiceId));
            Assert.Equal("Service not found.", exception.Message);
        }
    }
}
