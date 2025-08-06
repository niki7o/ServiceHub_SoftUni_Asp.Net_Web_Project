using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Common;
using ServiceHub.Common.Enum;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using ServiceHub.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.Services
{
  
    public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable) { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression) { }

      

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }

    

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }

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
                new ApplicationUser { Id = "testUserId2", UserName = "user2", Email = "user2@test.com" },
                new ApplicationUser { Id = "adminUserId", UserName = "admin", Email = "admin@test.com" }
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
                    ViewsCount = 10,
                    IsTemplate = false,
                    IsApproved = true,
                    CreatedByUserId = _users[0].Id
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
                    ViewsCount = 20,
                    IsTemplate = false,
                    IsApproved = true,
                    CreatedByUserId = _users[1].Id
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
                    ViewsCount = 5,
                    IsTemplate = false,
                    IsApproved = true,
                    CreatedByUserId = _users[0].Id
                },
                new Service
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Title = "Template D",
                    Description = "Desc D",
                    AccessType = AccessType.Free,
                    CategoryId = _categories[0].Id,
                    Category = _categories[0],
                    CreatedOn = DateTime.UtcNow.AddDays(-2),
                    ViewsCount = 0,
                    IsTemplate = true,
                    IsApproved = false,
                    CreatedByUserId = _users[1].Id
                },
                 new Service
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Title = "Approved Template E",
                    Description = "Desc E",
                    AccessType = AccessType.Free,
                    CategoryId = _categories[0].Id,
                    Category = _categories[0],
                    CreatedOn = DateTime.UtcNow.AddDays(-2),
                    ViewsCount = 0,
                    IsTemplate = true,
                    IsApproved = true,
                    CreatedByUserId = _users[1].Id
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
        public async Task ToggleFavorite_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            var userId = _users[0].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.ToggleFavorite(nonExistentServiceId, userId));
            Assert.Equal("Service not found.", exception.Message);
        }

        [Fact]
        public async Task ToggleFavorite_ShouldThrowInvalidOperationException_WhenServiceIsUnapprovedTemplate()
        {
            var serviceId = _services[3].Id;
            var userId = _users[0].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(_services[3]);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _servicesService.ToggleFavorite(serviceId, userId));
            Assert.Equal("Не може да добавяте неодобрени шаблони към любими.", exception.Message);
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
            var userId = _users[0].Id;

            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(newServiceFormModel.CategoryId)).ReturnsAsync(_categories[0]);
            _mockServiceRepo.Setup(repo => repo.AddAsync(It.IsAny<Service>())).Returns(Task.CompletedTask);
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.CreateAsync(newServiceFormModel, userId);

            _mockServiceRepo.Verify(repo => repo.AddAsync(It.Is<Service>(s => s.Title == newServiceFormModel.Title && s.Description == newServiceFormModel.Description && !s.IsTemplate && s.IsApproved && s.CreatedByUserId == userId)), Times.Once);
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
            var userId = _users[0].Id;

            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(newServiceFormModel.CategoryId)).ReturnsAsync((Category)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.CreateAsync(newServiceFormModel, userId));
            Assert.Equal("Invalid category selected.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.AddAsync(It.IsAny<Service>()), Times.Never);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateServiceAndSaveChanges_WhenAdmin()
        {
            var serviceToUpdateId = _services[0].Id;
            var updatedServiceFormModel = new ServiceFormModel
            {
                Title = "Updated Service A",
                Description = "Updated Desc A",
                CategoryId = _categories[1].Id,
                AccessType = AccessType.Premium
            };
            var editorId = _users[2].Id;

            var existingService = _services[0];
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToUpdateId)).ReturnsAsync(existingService);
            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(updatedServiceFormModel.CategoryId)).ReturnsAsync(_categories[1]);
            _mockServiceRepo.Setup(repo => repo.Update(It.IsAny<Service>()));
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.UpdateAsync(serviceToUpdateId, updatedServiceFormModel, editorId, true);

            Assert.Equal(updatedServiceFormModel.Title, existingService.Title);
            Assert.Equal(updatedServiceFormModel.Description, existingService.Description);
            Assert.Equal(updatedServiceFormModel.CategoryId, existingService.CategoryId);
            Assert.Equal(updatedServiceFormModel.AccessType, existingService.AccessType);
            Assert.NotNull(existingService.ModifiedOn);
            _mockServiceRepo.Verify(repo => repo.Update(existingService), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowUnauthorizedAccessException_WhenNotAdmin()
        {
            var serviceToUpdateId = _services[0].Id;
            var updatedServiceFormModel = new ServiceFormModel();
            var editorId = _users[0].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToUpdateId)).ReturnsAsync(_services[0]);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _servicesService.UpdateAsync(serviceToUpdateId, updatedServiceFormModel, editorId, false));
            Assert.Equal("You are not authorized to edit service settings.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenServiceIsTemplate()
        {
            var serviceToUpdateId = _services[3].Id;
            var updatedServiceFormModel = new ServiceFormModel();
            var editorId = _users[2].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToUpdateId)).ReturnsAsync(_services[3]);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _servicesService.UpdateAsync(serviceToUpdateId, updatedServiceFormModel, editorId, true));
            Assert.Equal("Templates cannot be edited via this method.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            var model = new ServiceFormModel();
            var editorId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.UpdateAsync(nonExistentServiceId, model, editorId, true));
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
            var editorId = _users[2].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToUpdateId)).ReturnsAsync(_services[0]);
            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(updatedServiceFormModel.CategoryId)).ReturnsAsync((Category)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.UpdateAsync(serviceToUpdateId, updatedServiceFormModel, editorId, true));
            Assert.Equal("Invalid category selected.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.Update(It.IsAny<Service>()), Times.Never);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteServiceAndSaveChanges_WhenAdmin()
        {
            var serviceToDeleteId = _services[0].Id;
            var serviceToDelete = _services[0];
            var deleterId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToDeleteId)).ReturnsAsync(serviceToDelete);
            _mockServiceRepo.Setup(repo => repo.Delete(It.IsAny<Service>()));
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.DeleteAsync(serviceToDeleteId, deleterId, true);

            _mockServiceRepo.Verify(repo => repo.Delete(serviceToDelete), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenNotAdmin()
        {
            var serviceToDeleteId = _services[0].Id;
            var deleterId = _users[0].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceToDeleteId)).ReturnsAsync(_services[0]);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _servicesService.DeleteAsync(serviceToDeleteId, deleterId, false));
            Assert.Equal("You are not authorized to delete this service.", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            var deleterId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.DeleteAsync(nonExistentServiceId, deleterId, true));
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
        public async Task AddReviewAsync_ShouldThrowInvalidOperationException_WhenServiceIsUnapprovedTemplate()
        {
            var serviceId = _services[3].Id;
            var userId = _users[0].Id;
            var reviewModel = new ReviewFormModel { Rating = 5, Comment = "Test" };

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(_services[3]);
            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(_users[0]);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _servicesService.AddReviewAsync(serviceId, userId, reviewModel));
            Assert.Equal("Не може да оставяте ревюта за неодобрени шаблони.", exception.Message);
        }

        [Fact]
        public async Task GetServiceForEditAsync_ShouldReturnServiceFormModel_WhenServiceExists_Admin()
        {
            var serviceId = _services[0].Id;
            var serviceToEdit = _services[0];
            var editorId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(serviceToEdit);

            var result = await _servicesService.GetServiceForEditAsync(serviceId, editorId, true);

            Assert.NotNull(result);
            Assert.Equal(serviceToEdit.Title, result.Title);
            Assert.Equal(serviceToEdit.Description, result.Description);
            Assert.Equal(serviceToEdit.CategoryId, result.CategoryId);
            Assert.Equal(serviceToEdit.AccessType, result.AccessType);
        }

        [Fact]
        public async Task GetServiceForEditAsync_ShouldThrowUnauthorizedAccessException_WhenNotAdmin()
        {
            var serviceId = _services[0].Id;
            var editorId = _users[0].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(_services[0]);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _servicesService.GetServiceForEditAsync(serviceId, editorId, false));
            Assert.Equal("You are not authorized to view this service for editing.", exception.Message);
        }

        [Fact]
        public async Task GetServiceForEditAsync_ShouldThrowInvalidOperationException_WhenServiceIsTemplate()
        {
            var serviceId = _services[3].Id;
            var editorId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(serviceId)).ReturnsAsync(_services[3]);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _servicesService.GetServiceForEditAsync(serviceId, editorId, true));
            Assert.Equal("Templates cannot be edited via this method.", exception.Message);
        }

        [Fact]
        public async Task GetServiceForEditAsync_ShouldThrowArgumentException_WhenServiceNotFound()
        {
            var nonExistentServiceId = Guid.NewGuid();
            var editorId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentServiceId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.GetServiceForEditAsync(nonExistentServiceId, editorId, true));
            Assert.Equal("Service not found.", exception.Message);
        }

        [Fact]
        public async Task AddServiceTemplateAsync_ShouldAddTemplateAndSaveChanges_WhenBusinessUser()
        {
            var newTemplateModel = new ServiceFormModel
            {
                Title = "New Template",
                Description = "Template Desc",
                CategoryId = _categories[0].Id,
                AccessType = AccessType.Free
            };
            var userId = _users[1].Id;
            var user = _users[1];
            user.LastServiceCreationDate = null;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(true);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(newTemplateModel.CategoryId)).ReturnsAsync(_categories[0]);
            _mockServiceRepo.Setup(repo => repo.AddAsync(It.IsAny<Service>())).Returns(Task.CompletedTask);
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);
            _mockUserManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            await _servicesService.AddServiceTemplateAsync(newTemplateModel, userId, false);

            _mockServiceRepo.Verify(repo => repo.AddAsync(It.Is<Service>(s => s.Title == newTemplateModel.Title && s.IsTemplate && !s.IsApproved && s.CreatedByUserId == userId)), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mockUserManager.Verify(um => um.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task AddServiceTemplateAsync_ShouldAddTemplateAndApprove_WhenAdmin()
        {
            var newTemplateModel = new ServiceFormModel
            {
                Title = "New Template Admin",
                Description = "Template Desc Admin",
                CategoryId = _categories[0].Id,
                AccessType = AccessType.Free
            };
            var userId = _users[2].Id;
            var user = _users[2];

            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);
            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(newTemplateModel.CategoryId)).ReturnsAsync(_categories[0]);
            _mockServiceRepo.Setup(repo => repo.AddAsync(It.IsAny<Service>())).Returns(Task.CompletedTask);
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.AddServiceTemplateAsync(newTemplateModel, userId, true);

            _mockServiceRepo.Verify(repo => repo.AddAsync(It.Is<Service>(s => s.Title == newTemplateModel.Title && s.IsTemplate && s.IsApproved && s.CreatedByUserId == userId)), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task AddServiceTemplateAsync_ShouldThrowInvalidOperationException_WhenBusinessUserExceedsDailyLimit()
        {
            var newTemplateModel = new ServiceFormModel
            {
                Title = "New Template",
                Description = "Template Desc",
                CategoryId = _categories[0].Id,
                AccessType = AccessType.Free
            };
            var userId = _users[1].Id;
            var user = _users[1];
            user.LastServiceCreationDate = DateTime.UtcNow.Date;

            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "BusinessUser")).ReturnsAsync(true);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);
            _mockCategoryRepo.Setup(repo => repo.GetByIdAsync(newTemplateModel.CategoryId)).ReturnsAsync(_categories[0]);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _servicesService.AddServiceTemplateAsync(newTemplateModel, userId, false));
            Assert.Equal("Бизнес потребител може да създава само по 1 услуга на ден.", exception.Message);
            _mockServiceRepo.Verify(repo => repo.AddAsync(It.IsAny<Service>()), Times.Never);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Never);
            _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        

        [Fact]
        public async Task ApproveServiceTemplateAsync_ShouldApproveTemplateAndSaveChanges()
        {
            var templateId = _services[3].Id;
            var adminId = _users[2].Id;

            var templateToApprove = _services[3];
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(templateId)).ReturnsAsync(templateToApprove);
            _mockServiceRepo.Setup(repo => repo.Update(It.IsAny<Service>()));
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.ApproveServiceTemplateAsync(templateId, adminId);

            Assert.False(templateToApprove.IsTemplate);
            Assert.True(templateToApprove.IsApproved);
            Assert.Equal(adminId, templateToApprove.ApprovedByUserId);
            Assert.NotNull(templateToApprove.ApprovedOn);
            _mockServiceRepo.Verify(repo => repo.Update(templateToApprove), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveServiceTemplateAsync_ShouldThrowArgumentException_WhenTemplateNotFound()
        {
            var nonExistentTemplateId = Guid.NewGuid();
            var adminId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentTemplateId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.ApproveServiceTemplateAsync(nonExistentTemplateId, adminId));
            Assert.Equal("Service template not found.", exception.Message);
        }

        [Fact]
        public async Task ApproveServiceTemplateAsync_ShouldThrowInvalidOperationException_WhenAlreadyApproved()
        {
            var templateId = _services[4].Id;
            var adminId = _users[2].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(templateId)).ReturnsAsync(_services[4]);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _servicesService.ApproveServiceTemplateAsync(templateId, adminId));
            Assert.Equal("Service is not a pending template or already approved.", exception.Message);
        }

        [Fact]
        public async Task RejectServiceTemplateAsync_ShouldDeleteTemplateAndSaveChanges()
        {
            var templateId = _services[3].Id;
            var adminId = _users[2].Id;

            var templateToReject = _services[3];
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(templateId)).ReturnsAsync(templateToReject);
            _mockServiceRepo.Setup(repo => repo.Delete(It.IsAny<Service>()));
            _mockServiceRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            await _servicesService.RejectServiceTemplateAsync(templateId, adminId);

            _mockServiceRepo.Verify(repo => repo.Delete(templateToReject), Times.Once);
            _mockServiceRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RejectServiceTemplateAsync_ShouldThrowArgumentException_WhenTemplateNotFound()
        {
            var nonExistentTemplateId = Guid.NewGuid();
            var adminId = _users[2].Id;
            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(nonExistentTemplateId)).ReturnsAsync((Service)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _servicesService.RejectServiceTemplateAsync(nonExistentTemplateId, adminId));
            Assert.Equal("Service template not found.", exception.Message);
        }

        [Fact]
        public async Task RejectServiceTemplateAsync_ShouldThrowInvalidOperationException_WhenAlreadyApproved()
        {
            var templateId = _services[4].Id;
            var adminId = _users[2].Id;

            _mockServiceRepo.Setup(repo => repo.GetByIdAsync(templateId)).ReturnsAsync(_services[4]);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _servicesService.RejectServiceTemplateAsync(templateId, adminId));
            Assert.Equal("Service is not a pending template or already approved.", exception.Message);
        }

        
        
    }
}
