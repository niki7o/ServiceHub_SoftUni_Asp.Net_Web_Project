using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceHub.Common.Enum;
using ServiceHub.Core.Models;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using ServiceHub.Services.Services.Repository;
using System;
using System.Collections.Generic; // Добавете този using
using System.Linq;
using System.Security.Claims; // Добавете този using
using System.Threading.Tasks;

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

       
        private async Task<string?> GetUserRoleCssClass(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains("Admin")) return "role-admin";
            if (roles.Contains("BusinessUser")) return "role-business";
          
            return null; 
        }


        public async Task<ServiceAllViewModel> GetAllAsync(string? categoryFilter = null, string? accessTypeFilter = null, string? filter = null, string? sort = null, string? currentUserId = null, int currentPage = 1, int servicesPerPage = 9)
        {
            try
            {
                _logger.LogInformation($"GetAllAsync: Извикване за потребител ID: {currentUserId ?? "N/A"}.");

                IQueryable<Service> servicesQuery = serviceRepo
                    .AllAsNoTracking()
                    .Include(s => s.Category)
                    .Include(s => s.Reviews)
                        .ThenInclude(r => r.User)
                    .Include(s => s.Favorites)
                    .Include(s => s.CreatedByUser);

                bool isCurrentUserAdmin = false;
                if (currentUserId != null)
                {
                    var user = await userManager.FindByIdAsync(currentUserId);
                    if (user != null)
                    {
                        isCurrentUserAdmin = await userManager.IsInRoleAsync(user, "Admin");
                        _logger.LogInformation($"GetAllAsync: Потребител {user.UserName} (ID: {currentUserId}) е администратор: {isCurrentUserAdmin}.");
                    }
                    else
                    {
                        _logger.LogWarning($"GetAllAsync: Потребител с ID {currentUserId} не е намерен.");
                    }
                }
                else
                {
                    _logger.LogInformation("GetAllAsync: Неавтентикиран потребител.");
                }


                if (!isCurrentUserAdmin)
                {
                    servicesQuery = servicesQuery.Where(s => s.IsApproved);
                    _logger.LogInformation("GetAllAsync: Прилага се филтър за не-администратори: IsApproved = true.");
                }
                else
                {
                    _logger.LogInformation("GetAllAsync: Потребителят е администратор. Не се прилага филтър за IsTemplate/IsApproved в GetAllAsync.");
                }

                var allCategories = await categoryRepo.AllAsNoTracking().OrderBy(c => c.Name).ToListAsync();
                var categoriesList = allCategories.Select(c => new SelectListItem { Value = c.Name, Text = c.Name }).ToList();
                categoriesList.Insert(0, new SelectListItem { Value = "", Text = "Всички Категории" });

                var allAccessTypes = Enum.GetNames(typeof(AccessType))
                                             .Select(name => new SelectListItem { Value = name, Text = name })
                                             .ToList();
                allAccessTypes.Insert(0, new SelectListItem { Value = "", Text = "Всички Типове Достъп" });

                if (!string.IsNullOrEmpty(categoryFilter) && categoryFilter != "Всички Категории" && categoryFilter != "All Categories")
                {
                    servicesQuery = servicesQuery.Where(s => s.Category != null && s.Category.Name == categoryFilter);
                    _logger.LogInformation($"GetAllAsync: Прилага се филтър за категория: '{categoryFilter}'.");
                }

                if (!string.IsNullOrEmpty(accessTypeFilter) && accessTypeFilter != "Всички Типове Достъп" && accessTypeFilter != "All Access Types")
                {
                    if (Enum.TryParse<AccessType>(accessTypeFilter, true, out AccessType parsedAccessType))
                    {
                        servicesQuery = servicesQuery.Where(s => s.AccessType == parsedAccessType);
                        _logger.LogInformation($"GetAllAsync: Прилага се филтър за тип достъп: '{accessTypeFilter}'.");
                    }
                    else
                    {
                        _logger.LogWarning($"GetAllAsync: Невалиден accessTypeFilter '{accessTypeFilter}'. Филтърът ще бъде игнориран.");
                    }
                }

                if (!string.IsNullOrEmpty(filter) && filter.ToLowerInvariant() == "favorite")
                {
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        servicesQuery = servicesQuery.Where(s => s.Favorites.Any(f => f.UserId == currentUserId));
                        _logger.LogInformation($"GetAllAsync: Прилага се филтър за любими услуги за потребител: '{currentUserId}'.");
                    }
                }

                servicesQuery = sort?.ToLowerInvariant() switch
                {
                    "az" => servicesQuery.OrderBy(s => s.Title),
                    "za" => servicesQuery.OrderByDescending(s => s.Title),
                    "recent" => servicesQuery.OrderByDescending(s => s.CreatedOn),
                    "mostviewed" => servicesQuery.OrderByDescending(s => s.ViewsCount),
                    _ => servicesQuery.OrderByDescending(s => s.CreatedOn)
                };
                _logger.LogInformation($"GetAllAsync: Прилага се сортиране: '{sort ?? "recent"}'.");

                int totalServicesCount = await servicesQuery.CountAsync();
                _logger.LogInformation($"GetAllAsync: Общ брой услуги след филтриране: {totalServicesCount}.");

                var services = await servicesQuery
                    .Skip((currentPage - 1) * servicesPerPage)
                    .Take(servicesPerPage)
                    .ToListAsync();

                _logger.LogInformation($"GetAllAsync: Извлечени {services.Count} услуги за страница {currentPage}.");

                var serviceViewModels = new List<ServiceViewModel>();
                foreach (var s in services)
                {
                    var serviceViewModel = new ServiceViewModel
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        AccessType = s.AccessType,
                        CategoryName = s.Category?.Name,
                        CreatedByUserName = s.CreatedByUser?.UserName ?? "Unknown",
                        CreatedByUserId = s.CreatedByUserId,
                        CreatedByUserRoleCssClass = s.CreatedByUserId != null ? await GetUserRoleCssClass(s.CreatedByUserId) : null,
                        ReviewCount = s.Reviews.Count,
                        AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0,
                        IsFavorite = currentUserId != null && s.Favorites.Any(f => f.UserId == currentUserId),
                        ViewsCount = s.ViewsCount,
                        IsTemplate = s.IsTemplate,
                        IsApproved = s.IsApproved
                    };

                    var reviewViewModels = new List<ReviewViewModel>();
                    foreach (var r in s.Reviews)
                    {
                        reviewViewModels.Add(new ReviewViewModel
                        {
                            Id = r.Id,
                            ServiceId = r.ServiceId,
                            UserName = r.User?.UserName ?? "Anonymous",
                            UserRoleCssClass = r.UserId != null ? await GetUserRoleCssClass(r.UserId) : null, 
                            Rating = r.Rating,
                            Comment = r.Comment,
                            CreatedOn = r.CreatedOn,
                            IsAuthor = currentUserId != null && r.UserId == currentUserId
                        });
                    }
                    serviceViewModel.Reviews = reviewViewModels.OrderByDescending(r => r.CreatedOn).ToList();
                    serviceViewModels.Add(serviceViewModel);
                }

                return new ServiceAllViewModel
                {
                    Services = serviceViewModels,
                    CurrentPage = currentPage,
                    PageSize = servicesPerPage,
                    TotalServicesCount = totalServicesCount,
                    TotalPages = (int)Math.Ceiling(totalServicesCount / (double)servicesPerPage),
                    Categories = categoriesList,
                    AccessTypes = allAccessTypes,
                    CurrentCategoryFilter = categoryFilter,
                    CurrentAccessTypeFilter = accessTypeFilter,
                    CurrentSort = sort,
                    CurrentFilter = filter
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllAsync: Възникна грешка при извличане на всички услуги. Филтри: Категория='{Category}', Достъп='{Access}', Филтър='{Filter}', Сортиране='{Sort}', Потребител='{UserId}', Страница={Page}",
                    categoryFilter, accessTypeFilter, filter, sort, currentUserId, currentPage);

                throw;
            }
        }


        public async Task<ServiceViewModel> GetByIdAsync(Guid id, string? currentUserId = null, int reviewPage = 1, int reviewsPerPage = 2)
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

            var allReviews = new List<ReviewViewModel>();
            foreach (var r in service.Reviews)
            {
                allReviews.Add(new ReviewViewModel
                {
                    Id = r.Id,
                    ServiceId = r.ServiceId,
                    UserName = r.User?.UserName ?? "Anonymous",
                    UserRoleCssClass = r.UserId != null ? await GetUserRoleCssClass(r.UserId) : null, 
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn,
                    IsAuthor = currentUserId != null && r.UserId == currentUserId
                });
            }
            allReviews = allReviews.OrderByDescending(r => r.CreatedOn).ToList();

            int totalReviewCount = allReviews.Count;
            int totalReviewPages = (int)Math.Ceiling((double)totalReviewCount / reviewsPerPage);


            _logger.LogInformation($"Review Pagination Debug: ServiceId={id}, totalReviewCount={totalReviewCount}, reviewsPerPage={reviewsPerPage}, currentReviewPage (before adjustment)={reviewPage}");


            if (reviewPage < 1)
                reviewPage = 1;
            if (reviewPage > totalReviewPages && totalReviewPages > 0)
                reviewPage = totalReviewPages;
            if (totalReviewPages == 0)
                reviewPage = 1;

            _logger.LogInformation($"Review Pagination Debug: ServiceId={id}, currentReviewPage (after adjustment)={reviewPage}, totalReviewPages={totalReviewPages}");


            var paginatedReviews = allReviews
                                        .Skip((reviewPage - 1) * reviewsPerPage)
                                        .Take(reviewsPerPage)
                                        .ToList();

            _logger.LogInformation($"Review Pagination Debug: ServiceId={id}, paginatedReviews.Count={paginatedReviews.Count}");

            string? createdByUserRoleCssClass = service.CreatedByUserId != null ? await GetUserRoleCssClass(service.CreatedByUserId) : null; 

            return new ServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                AccessType = service.AccessType,
                CategoryName = service.Category?.Name,
                CreatedByUserName = service.CreatedByUser?.UserName ?? "Unknown",
                CreatedByUserId = service.CreatedByUserId, 
                CreatedByUserRoleCssClass = createdByUserRoleCssClass, 
                ReviewCount = totalReviewCount,
                AverageRating = service.Reviews.Any() ? service.Reviews.Average(r => r.Rating) : 0,
                Reviews = paginatedReviews,
                IsFavorite = isFavorite,
                ViewsCount = service.ViewsCount,
                IsTemplate = service.IsTemplate,
                IsApproved = service.IsApproved,
                CurrentReviewPage = reviewPage,
                TotalReviewPages = totalReviewPages,
                ReviewsPerPage = reviewsPerPage
            };
        }

        public async Task CreateAsync(ServiceFormModel model, string userId)
        {
            _logger.LogInformation($"CreateAsync: Извикване за потребител ID: {userId} за услуга '{model.Title}'.");

            var category = await categoryRepo.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                _logger.LogWarning($"CreateAsync: Невалидна категория с ID {model.CategoryId}.");
                throw new ArgumentException("Invalid category selected.");
            }

            var service = new Service
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                AccessType = model.AccessType,
                CreatedOn = DateTime.UtcNow,
                IsTemplate = false,
                IsApproved = true,
                CreatedByUserId = userId
            };

            await serviceRepo.AddAsync(service);
            await serviceRepo.SaveChangesAsync();
            _logger.LogInformation($"CreateAsync: Услуга '{service.Title}' (ID: {service.Id}) създадена. IsTemplate: {service.IsTemplate}, IsApproved: {service.IsApproved}.");
        }

        public async Task UpdateAsync(Guid id, ServiceFormModel model, string editorId, bool isAdmin)
        {
            _logger.LogInformation($"UpdateAsync: Извикване за услуга ID: {id} от потребител ID: {editorId} (isAdmin: {isAdmin}).");

            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                _logger.LogWarning($"UpdateAsync: Услуга с ID {id} не е намерена.");
                throw new ArgumentException("Service not found.");
            }

            if (!isAdmin)
            {
                _logger.LogWarning($"UpdateAsync: Потребител {editorId} не е оторизиран да редактира услуга ID: {id}.");
                throw new UnauthorizedAccessException("You are not authorized to edit service settings.");
            }

         
            if (service.IsTemplate)
            {
                _logger.LogWarning($"UpdateAsync: Опит за редактиране на шаблон ID: {id} чрез неподходящ метод.");
                throw new InvalidOperationException("Templates cannot be edited via this method.");
            }

            var category = await categoryRepo.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                _logger.LogWarning($"UpdateAsync: Невалидна категория с ID {model.CategoryId}.");
                throw new ArgumentException("Invalid category selected.");
            }

            service.Title = model.Title;
            service.Description = model.Description;
            service.CategoryId = model.CategoryId;
            service.AccessType = model.AccessType;
            service.ModifiedOn = DateTime.UtcNow;

            serviceRepo.Update(service);
            await serviceRepo.SaveChangesAsync();
            _logger.LogInformation($"UpdateAsync: Услуга '{service.Title}' (ID: {service.Id}) обновена.");
        }

        public async Task DeleteAsync(Guid id, string deleterId, bool isAdmin)
        {
            _logger.LogInformation($"DeleteAsync: Извикване за услуга ID: {id} от потребител ID: {deleterId} (isAdmin: {isAdmin}).");

            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                _logger.LogWarning($"DeleteAsync: Услуга с ID {id} не е намерена.");
                throw new ArgumentException("Service not found.");
            }

            if (!isAdmin)
            {
                _logger.LogWarning($"DeleteAsync: Потребител {deleterId} не е оторизиран да изтрие услуга ID: {id}.");
                throw new UnauthorizedAccessException("You are not authorized to delete this service.");
            }

            serviceRepo.Delete(service);
            await serviceRepo.SaveChangesAsync();
            _logger.LogInformation($"DeleteAsync: Услуга '{service.Title}' (ID: {service.Id}) изтрита.");
        }

        public async Task AddReviewAsync(Guid serviceId, string userId, ReviewFormModel model)
        {
            _logger.LogInformation($"AddReviewAsync: Извикване за услуга ID: {serviceId} от потребител ID: {userId}.");

            var service = await serviceRepo.GetByIdAsync(serviceId);
            if (service == null)
            {
                _logger.LogWarning($"AddReviewAsync: Услуга с ID {serviceId} не е намерена.");
                throw new ArgumentException("Service not found.");
            }

            if (service.IsTemplate && !service.IsApproved)
            {
                _logger.LogWarning($"AddReviewAsync: Опит за добавяне на ревю към неодобрен шаблон ID: {serviceId}.");
                throw new InvalidOperationException("Не може да оставяте ревюта за неодобрени шаблони.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"AddReviewAsync: Потребител с ID {userId} не е намерен.");
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
            _logger.LogInformation($"AddReviewAsync: Ревю добавено за услуга ID: {serviceId} от потребител ID: {userId}. Рейтинг: {model.Rating}.");
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


        public async Task<PaginatedServiceTemplatesResult> GetAllPendingTemplatesAsync(int pageNumber, int pageSize)
        {
            var query = serviceRepo.AllAsNoTracking()
                .Where(s => s.IsTemplate && !s.IsApproved)
                .Include(s => s.Category)
                .Include(s => s.CreatedByUser)
                .OrderByDescending(s => s.CreatedOn);

            int totalCount = await query.CountAsync();

            var templates = new List<ServiceViewModel>();
            foreach (var s in await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync())
            {
                templates.Add(new ServiceViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    CategoryName = s.Category?.Name,
                    AccessType = s.AccessType,
                    CreatedByUserName = s.CreatedByUser?.UserName,
                    CreatedByUserId = s.CreatedByUserId, 
                    CreatedByUserRoleCssClass = s.CreatedByUserId != null ? await GetUserRoleCssClass(s.CreatedByUserId) : null, 
                    IsTemplate = s.IsTemplate,
                    IsApproved = s.IsApproved
                });
            }

            return new PaginatedServiceTemplatesResult
            {
                Templates = templates,
                TotalCount = totalCount
            };
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

        public async Task<PaginatedServicesResult> GetCreatedServicesByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            var query = serviceRepo.AllAsNoTracking()
                .Where(s => s.CreatedByUserId == userId && s.IsApproved && !s.IsTemplate)
                .Include(s => s.Category)
                .Include(s => s.Reviews)
                .Include(s => s.CreatedByUser);

            int totalCount = await query.CountAsync();

            var services = new List<ServiceViewModel>();
            foreach (var s in await query.OrderByDescending(s => s.CreatedOn).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync())
            {
                services.Add(new ServiceViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    AccessType = s.AccessType,
                    CategoryName = s.Category?.Name,
                    CreatedByUserName = s.CreatedByUser?.UserName,
                    CreatedByUserId = s.CreatedByUserId, 
                    CreatedByUserRoleCssClass = s.CreatedByUserId != null ? await GetUserRoleCssClass(s.CreatedByUserId) : null, 
                    ReviewCount = s.Reviews.Count,
                    AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0,
                    ViewsCount = s.ViewsCount
                });
            }

            return new PaginatedServicesResult
            {
                Services = services,
                TotalCount = totalCount
            };
        }

        public async Task<PaginatedReviewsResult> GetReviewsByUserIdAsync(string userId, int pageNumber, int pageSize)
        {
            var query = reviewRepo.AllAsNoTracking()
                .Where(r => r.UserId == userId)
                .Include(r => r.Service)
                    .ThenInclude(s => s.Category)
                .Include(r => r.User);

            int totalCount = await query.CountAsync();

            var reviews = new List<ReviewViewModel>();
            foreach (var r in await query.OrderByDescending(r => r.CreatedOn).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync())
            {
                reviews.Add(new ReviewViewModel
                {
                    Id = r.Id,
                    ServiceId = r.Service.Id, 
                    ServiceName = r.Service.Title,
                    UserName = r.User?.UserName ?? "Anonymous",
                    UserRoleCssClass = r.UserId != null ? await GetUserRoleCssClass(r.UserId) : null,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn
                });
            }

            return new PaginatedReviewsResult
            {
                Reviews = reviews,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<ServiceViewModel>> GetFavoriteServicesByUserIdAsync(string userId)
        {
            var services = new List<ServiceViewModel>();
            var query = favoriteRepo.AllAsNoTracking()
                .Where(f => f.UserId == userId)
                .Include(f => f.Service)
                    .ThenInclude(s => s.Category)
                .Include(f => f.Service)
                    .ThenInclude(s => s.Reviews)
                .Include(f => f.Service)
                    .ThenInclude(s => s.CreatedByUser);

            foreach (var f in await query.ToListAsync())
            {
                services.Add(new ServiceViewModel
                {
                    Id = f.Service.Id,
                    Title = f.Service.Title,
                    Description = f.Service.Description,
                    AccessType = f.Service.AccessType,
                    CategoryName = f.Service.Category?.Name,
                    CreatedByUserName = f.Service.CreatedByUser?.UserName,
                    CreatedByUserId = f.Service.CreatedByUserId, 
                    CreatedByUserRoleCssClass = f.Service.CreatedByUserId != null ? await GetUserRoleCssClass(f.Service.CreatedByUserId) : null, 
                    ReviewCount = f.Service.Reviews.Count,
                    AverageRating = f.Service.Reviews.Any() ? f.Service.Reviews.Average(r => r.Rating) : 0,
                    ViewsCount = f.Service.ViewsCount,
                    IsFavorite = true
                });
            }
            return services;
        }

        public async Task<int> GetApprovedServicesCountByUserIdAsync(string userId)
        {
            return await serviceRepo.AllAsNoTracking()
                .CountAsync(s => s.CreatedByUserId == userId && s.IsApproved && !s.IsTemplate);
        }

        public async Task<IEnumerable<ServiceViewModel>> SearchServicesByTitleAsync(string searchTerm)
        {
            var services = new List<ServiceViewModel>();
            var query = serviceRepo.AllAsNoTracking()
                .Where(s => s.Title.Contains(searchTerm) && !s.IsTemplate && s.IsApproved)
                .Include(s => s.Category)
                .Include(s => s.CreatedByUser) 
                .Include(s => s.Reviews); 

            foreach (var s in await query.ToListAsync())
            {
                services.Add(new ServiceViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    CategoryName = s.Category?.Name,
                    AccessType = s.AccessType,
                    CreatedByUserName = s.CreatedByUser?.UserName,
                    CreatedByUserId = s.CreatedByUserId, 
                    CreatedByUserRoleCssClass = s.CreatedByUserId != null ? await GetUserRoleCssClass(s.CreatedByUserId) : null, 
                    ReviewCount = s.Reviews.Count,
                    AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0,
                    ViewsCount = s.ViewsCount
                });
            }
            return services;
        }
    }
}
