using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public ServicesService(
            IRepository<Service> serviceRepo,
            IRepository<Category> categoryRepo,
            IRepository<Review> reviewRepo,
            IRepository<Favorite> favoriteRepo,
            UserManager<ApplicationUser> userManager)
        {
            this.serviceRepo = serviceRepo;
            this.categoryRepo = categoryRepo;
            this.reviewRepo = reviewRepo;
            this.favoriteRepo = favoriteRepo;
            this.userManager = userManager;
        }

        public async Task<IEnumerable<ServiceViewModel>> GetAllAsync(string? filter = null, string? sort = null, string? currentUserId = null)
        {
            IQueryable<Service> servicesQuery = serviceRepo
                .All()
                .Include(s => s.Category)
                .Include(s => s.Reviews)
                .Include(s => s.Favorites);

            if (!string.IsNullOrEmpty(filter))
            {
                string lowerFilter = filter.ToLower();
                switch (lowerFilter)
                {
                    case "free":
                        servicesQuery = servicesQuery.Where(s => s.AccessType == AccessType.Free);
                        break;
                    case "partial":
                        servicesQuery = servicesQuery.Where(s => s.AccessType == AccessType.Partial);
                        break;
                    case "premium":
                        servicesQuery = servicesQuery.Where(s => s.AccessType == AccessType.Premium);
                        break;
                    case "favorite":
                        if (!string.IsNullOrEmpty(currentUserId))
                        {
                            servicesQuery = servicesQuery.Where(s => s.Favorites.Any(f => f.UserId == currentUserId));
                        }
                        break;
                }
            }

            servicesQuery = sort?.ToLower() switch
            {
                "az" => servicesQuery.OrderBy(s => s.Title),
                "za" => servicesQuery.OrderByDescending(s => s.Title),
                "recent" => servicesQuery.OrderByDescending(s => s.CreatedOn),
                _ => servicesQuery
            };

            var services = await servicesQuery.ToListAsync();

            return services.Select(s => new ServiceViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                AccessType = s.AccessType,
                IsBusinessOnly = s.IsBusinessOnly,
                CategoryName = s.Category?.Name,
                ReviewCount = s.Reviews.Count,
                AverageRating = s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0,
                IsFavorite = currentUserId != null && s.Favorites.Any(f => f.UserId == currentUserId)
            }).ToList();
        }

        public async Task<ServiceViewModel> GetByIdAsync(Guid id, string? currentUserId = null)
        {
            var service = await serviceRepo
                .All()
                .Include(s => s.Category)
                .Include(s => s.Reviews)
                    .ThenInclude(r => r.User)
                .Include(s => s.Favorites)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {

                throw new ArgumentException("Service not found.");
            }

            bool isFavorite = false;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                isFavorite = service.Favorites.Any(f => f.UserId == currentUserId);
            }

            return new ServiceViewModel
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                IsBusinessOnly = service.IsBusinessOnly,
                AccessType = service.AccessType,
                CategoryName = service.Category?.Name,
                ReviewCount = service.Reviews.Count,
                AverageRating = service.Reviews.Any() ? service.Reviews.Average(r => r.Rating) : 0,
                Reviews = service.Reviews.Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    ServiceId = r.ServiceId,
                    UserName = r.User.UserName!,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedOn = r.CreatedOn
                }).OrderByDescending(r => r.CreatedOn).ToList(),
                IsFavorite = isFavorite
            };
        }

        public async Task CreateAsync(ServiceFormModel model)
        {
            var category = await categoryRepo.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Invalid category selected.");
            }

            var service = new Service
            {
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                IsBusinessOnly = model.IsBusinessOnly,
                AccessType = model.AccessType,
                CreatedOn = DateTime.UtcNow
            };

            await serviceRepo.AddAsync(service);
            await serviceRepo.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, ServiceFormModel model)
        {
            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
            }

            var category = await categoryRepo.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Invalid category selected.");
            }

            service.Title = model.Title;
            service.Description = model.Description;
            service.CategoryId = model.CategoryId;
            service.IsBusinessOnly = model.IsBusinessOnly;
            service.AccessType = model.AccessType;
            service.ModifiedOn = DateTime.UtcNow;

            serviceRepo.Update(service);
            await serviceRepo.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
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

        public async Task<ServiceFormModel> GetServiceForEditAsync(Guid id)
        {
            var service = await serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                throw new ArgumentException("Service not found.");
            }

            return new ServiceFormModel
            {
                Title = service.Title,
                Description = service.Description,
                IsBusinessOnly = service.IsBusinessOnly,
                AccessType = service.AccessType,
                CategoryId = service.CategoryId
            };
        }
    }
}