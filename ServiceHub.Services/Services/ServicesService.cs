using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceHub.Common.Enum;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;


namespace ServiceHub.Services.Services
{
    public class ServicesService : IServiceService
    {
        private readonly IRepository<Service> serviceRepo;
        private readonly IRepository<Category> categoryRepo;
        private readonly IRepository<Review> reviewRepo;
        private readonly IRepository<Favorite> favoriteRepo;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<ServicesService> _logger;

        public ServicesService(
            IRepository<Service> serviceRepo,
            IRepository<Category> categoryRepo,
            IRepository<Review> reviewRepo,
            IRepository<Favorite> favoriteRepo,
            UserManager<ApplicationUser> userManager,
            ILogger<ServicesService> logger)
        {
            this.serviceRepo = serviceRepo;
            this.categoryRepo = categoryRepo;
            this.reviewRepo = reviewRepo;
            this.favoriteRepo = favoriteRepo;
            this.userManager = userManager;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceViewModel>> GetAllAsync(string? categoryFilter = null, string? accessTypeFilter = null, string? filter = null, string? sort = null, string? currentUserId = null)
        {
            IQueryable<Service> servicesQuery = serviceRepo
                .AllAsNoTracking()
                .Include(s => s.Category)
                .Include(s => s.Reviews)
                    .ThenInclude(r => r.User)
                .Include(s => s.Favorites)
                .Include(s => s.CreatedByUser);

            if (currentUserId != null)
            {
                var user = await userManager.FindByIdAsync(currentUserId);
                if (user != null && !(await userManager.IsInRoleAsync(user, "Admin")))
                {
                    servicesQuery = servicesQuery.Where(s => !s.IsTemplate && s.IsApproved);
                }
            }
            else
            {
                servicesQuery = servicesQuery.Where(s => !s.IsTemplate && s.IsApproved);
            }

            if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "All Categories")
            {
                servicesQuery = servicesQuery.Where(s => s.Category != null && s.Category.Name == categoryFilter);
            }

            if (!string.IsNullOrEmpty(accessTypeFilter) && accessTypeFilter != "All Access Types")
            {
                if (Enum.TryParse<AccessType>(accessTypeFilter, true, out AccessType parsedAccessType))
                {
                    servicesQuery = servicesQuery.Where(s => s.AccessType == parsedAccessType);
                }
            }

            if (!string.IsNullOrEmpty(filter))
            {
                string lowerFilter = filter.ToLowerInvariant();
                switch (lowerFilter)
                {
                    case "favorite":
                        if (!string.IsNullOrEmpty(currentUserId))
                        {
                            servicesQuery = servicesQuery.Where(s => s.Favorites.Any(f => f.UserId == currentUserId));
                        }
                        break;
                }
            }

            servicesQuery = sort?.ToLowerInvariant() switch
            {
                "az" => servicesQuery.OrderBy(s => s.Title),
                "za" => servicesQuery.OrderByDescending(s => s.Title),
                "recent" => servicesQuery.OrderByDescending(s => s.CreatedOn),
                "mostviewed" => servicesQuery.OrderByDescending(s => s.ViewsCount),
                _ => servicesQuery
            };

            var services = await servicesQuery.ToListAsync();

            return services.Select(s => new ServiceViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                AccessType = s.AccessType,
                CategoryName = s.Category?.Name,
                // Removed IsPremium
                CreatedByUserName = s.CreatedByUser?.UserName ?? "Unknown",
                ReviewCount = s.Reviews.Count,
                AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0,
                IsFavorite = currentUserId != null && s.Favorites.Any(f => f.UserId == currentUserId),
                ViewsCount = s.ViewsCount,
                Reviews = s.Reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    ServiceId = r.ServiceId,
                    UserName = r.User?.UserName ?? "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn
                }).OrderByDescending(r => r.CreatedOn).ToList(),
                IsTemplate = s.IsTemplate,
                IsApproved = s.IsApproved
            }).ToList();
        }

        public async Task<ServiceViewModel> GetByIdAsync(Guid id, string? currentUserId = null)
        {
            _logger.LogInformation($"GetByIdAsync: Извличане на услуга с ID: {id} за потребител: {currentUserId}");

            var service = await serviceRepo
                .AllAsNoTracking()
                .Include(s => s.Category)
                .Include(s => s.Reviews)
                    .ThenInclude(r => r.User)
                .Include(s => s.Favorites)
                .Include(s => s.CreatedByUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                _logger.LogWarning($"GetByIdAsync: Услуга с ID {id} не е намерена. Хвърля се ArgumentException.");
                throw new ArgumentException("Service not found.");
            }

            if (service.IsTemplate && !service.IsApproved)
            {
                if (currentUserId == null)
                {
                    throw new UnauthorizedAccessException("Unauthenticated users cannot view unapproved templates.");
                }

                var currentUser = await userManager.FindByIdAsync(currentUserId);
                if (currentUser == null)
                {
                    throw new ArgumentException("Current user not found.");
                }

                bool isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");

                if (service.CreatedByUserId != currentUserId && !isAdmin)
                {
                    throw new UnauthorizedAccessException("You are not authorized to view this unapproved template.");
                }
            }

            bool isFavorite = false;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                isFavorite = service.Favorites.Any(f => f.UserId == currentUserId);
            }

            _logger.LogInformation($"GetByIdAsync: ServiceId: {service.Id}. ViewsCount извлечен от DB: {service.ViewsCount}");

            return new ServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                AccessType = service.AccessType,
                CategoryName = service.Category?.Name,
                // Removed IsPremium
                CreatedByUserName = service.CreatedByUser?.UserName ?? "Unknown",
                ReviewCount = service.Reviews.Count,
                AverageRating = service.Reviews.Any() ? service.Reviews.Average(r => r.Rating) : 0,
                Reviews = service.Reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    ServiceId = r.ServiceId,
                    UserName = r.User?.UserName ?? "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn
                }).OrderByDescending(r => r.CreatedOn).ToList(),
                IsFavorite = isFavorite,
                ViewsCount = service.ViewsCount,
                IsTemplate = service.IsTemplate,
                IsApproved = service.IsApproved
            };
        }

        public async Task CreateAsync(ServiceFormModel model, string userId)
        {
            var category = await categoryRepo.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Invalid category selected.");
            }

            var service = new Service
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                AccessType = model.AccessType,
                // Removed IsPremium
                CreatedOn = DateTime.UtcNow,
                IsTemplate = false,
                IsApproved = true,
                CreatedByUserId = userId
            };

            await serviceRepo.AddAsync(service);
            await serviceRepo.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, ServiceFormModel model, string editorId, bool isAdmin)
        {
            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
            }

            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit service settings.");
            }

            if (service.IsTemplate)
            {
                throw new InvalidOperationException("Templates cannot be edited via this method. Use approval/rejection.");
            }

            var category = await categoryRepo.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Invalid category selected.");
            }

            service.Title = model.Title;
            service.Description = model.Description;
            service.CategoryId = model.CategoryId;
            service.AccessType = model.AccessType;
            // Removed IsPremium
            service.ModifiedOn = DateTime.UtcNow;

            serviceRepo.Update(service);
            await serviceRepo.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, string deleterId, bool isAdmin)
        {
            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
            }

            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this service.");
            }

            serviceRepo.Delete(service);
            await serviceRepo.SaveChangesAsync();
        }

        public async Task AddReviewAsync(Guid serviceId, string userId, ReviewFormModel model)
        {
            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
            }

            if (service.IsTemplate && !service.IsApproved)
            {
                throw new InvalidOperationException("Не може да оставяте ревюта за неодобрени шаблони.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
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

            await reviewRepo.AddAsync(review);
            await reviewRepo.SaveChangesAsync();
        }

        public async Task IncrementViewsCount(Guid serviceId)
        {
            _logger.LogInformation($"IncrementViewsCount: Опит за увеличаване на броя преглеждания за ServiceId: {serviceId}");

            var service = await serviceRepo.All().AsTracking().FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
            {
                _logger.LogWarning($"IncrementViewsCount: Услуга с ID {serviceId} не е намерена. Не може да се увеличи броячът.");
                return;
            }

            if (!service.IsTemplate && service.IsApproved)
            {
                _logger.LogInformation($"IncrementViewsCount: ServiceId: {service.Id}. ViewsCount преди: {service.ViewsCount}");
                service.ViewsCount++;
                _logger.LogInformation($"IncrementViewsCount: ServiceId: {service.Id}. ViewsCount след увеличение: {service.ViewsCount}");
                try
                {
                    await serviceRepo.SaveChangesAsync();
                    _logger.LogInformation($"IncrementViewsCount: Успешно запазен нов ViewsCount за ServiceId: {serviceId}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"IncrementViewsCount: Грешка при запазване на ViewsCount за ServiceId: {serviceId}.");
                }
            }
            else
            {
                _logger.LogInformation($"IncrementViewsCount: ServiceId: {service.Id} е шаблон или не е одобрена. Броячът на преглеждания няма да бъде увеличен.");
            }
        }

        public async Task ToggleFavorite(Guid serviceId, string userId)
        {
            _logger.LogInformation($"Опит за превключване на любим статус за ServiceId: {serviceId} от UserId: {userId}");

            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
            {
                _logger.LogWarning($"ToggleFavorite: Service with ID {serviceId} not found.");
                throw new ArgumentException("Service not found.");
            }

            if (service.IsTemplate && !service.IsApproved)
            {
                _logger.LogWarning($"ToggleFavorite: Attempt to favorite unapproved template ServiceId: {serviceId} by UserId: {userId}.");
                throw new InvalidOperationException("Не може да добавяте неодобрени шаблони към любими.");
            }

            var existingFavorite = await favoriteRepo.All()
                                                     .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceId == serviceId);

            if (existingFavorite != null)
            {
                favoriteRepo.Delete(existingFavorite);
                _logger.LogInformation($"Премахнат любим статус за ServiceId: {serviceId} от UserId: {userId}");
            }
            else
            {
                var newFavorite = new Favorite
                {
                    UserId = userId,
                    ServiceId = serviceId,
                    CreatedOn = DateTime.UtcNow
                };
                await favoriteRepo.AddAsync(newFavorite);
                _logger.LogInformation($"Добавен любим статус за ServiceId: {serviceId} от UserId: {userId}");
            }
            await favoriteRepo.SaveChangesAsync();
            _logger.LogInformation($"Статусът на любими е запазен в базата данни за ServiceId: {serviceId} от UserId: {userId}");
        }

        public async Task<ServiceFormModel> GetServiceForEditAsync(Guid id, string editorId, bool isAdmin)
        {
            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
            }

            if (!isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this service for editing.");
            }

            if (service.IsTemplate)
            {
                throw new InvalidOperationException("Templates cannot be edited via this method.");
            }

            return new ServiceFormModel
            {
                Title = service.Title,
                Description = service.Description,
                AccessType = service.AccessType,
                CategoryId = service.CategoryId
                // Removed IsPremium
            };
        }

        public async Task AddServiceTemplateAsync(ServiceFormModel model, string userId, bool isAdmin)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (!isAdmin && await userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                if (user.LastServiceCreationDate.HasValue && user.LastServiceCreationDate.Value.Date == DateTime.UtcNow.Date)
                {
                    throw new InvalidOperationException("Бизнес потребител може да създава само по 1 услуга на ден.");
                }
            }

            var category = await categoryRepo.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Invalid category selected.");
            }

            var service = new Service
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                AccessType = model.AccessType,
                // Removed IsPremium
                CategoryId = model.CategoryId,
                CreatedByUserId = userId,
                CreatedOn = DateTime.UtcNow,
                IsTemplate = true,
                IsApproved = isAdmin
            };

            await serviceRepo.AddAsync(service);
            await serviceRepo.SaveChangesAsync();

            if (!isAdmin && await userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                user.LastServiceCreationDate = DateTime.UtcNow;
                await userManager.UpdateAsync(user);
            }
        }

        public async Task<IEnumerable<ServiceViewModel>> GetAllPendingTemplatesAsync()
        {
            return await serviceRepo.AllAsNoTracking()
                .Where(s => s.IsTemplate && !s.IsApproved)
                .Include(s => s.Category)
                .Include(s => s.CreatedByUser)
                .Select(s => new ServiceViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    CategoryName = s.Category.Name,
                    AccessType = s.AccessType,
                    // Removed IsPremium
                    CreatedByUserName = s.CreatedByUser.UserName,
                    IsTemplate = s.IsTemplate,
                    IsApproved = s.IsApproved
                })
                .ToListAsync();
        }

        public async Task ApproveServiceTemplateAsync(Guid serviceId, string adminId)
        {
            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new ArgumentException("Service template not found.");
            }

            if (!service.IsTemplate || service.IsApproved)
            {
                throw new InvalidOperationException("Service is not a pending template or already approved.");
            }

            service.IsApproved = true;
            service.IsTemplate = false;
            service.ModifiedOn = DateTime.UtcNow;
            service.ApprovedByUserId = adminId;
            service.ApprovedOn = DateTime.UtcNow;

            serviceRepo.Update(service);
            await serviceRepo.SaveChangesAsync();
        }

        public async Task RejectServiceTemplateAsync(Guid serviceId, string adminId)
        {
            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new ArgumentException("Service template not found.");
            }

            if (!service.IsTemplate || service.IsApproved)
            {
                throw new InvalidOperationException("Service is not a pending template or already approved.");
            }

            serviceRepo.Delete(service);
            await serviceRepo.SaveChangesAsync();
        }
    }
}