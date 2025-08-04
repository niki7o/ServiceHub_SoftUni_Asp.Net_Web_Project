using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using ServiceHub.Core.Models.Reviews; 
using ServiceHub.Core.Models.Service; 
using ServiceHub.Core.Models.User;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServiceHub.Areas.Identity.Controllers 
{
    [Authorize]
    [Area("Identity")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IServiceService serviceService;

        public UserController(UserManager<ApplicationUser> userManager, IServiceService serviceService)
        {
            this.userManager = userManager;
            this.serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> UserProfile(string? id, int createdServicesPage = 1, int reviewsPage = 1)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser user;

            if (string.IsNullOrEmpty(id))
            {
                user = await userManager.FindByIdAsync(currentUserId);
                if (user == null)
                {
                    return NotFound();
                }
            }
            else
            {
                user = await userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                if (id != currentUserId && !await userManager.IsInRoleAsync(await userManager.FindByIdAsync(currentUserId), "Admin"))
                {
                    return Forbid();
                }
            }

            IList<string> roles = await userManager.GetRolesAsync(user);

            
            int createdServicesPageSize = 5;
            int reviewsPageSize = 5;
  var paginatedCreatedServicesResult = await serviceService.GetCreatedServicesByUserIdAsync(user.Id, createdServicesPage, createdServicesPageSize);
            IEnumerable<ServiceViewModel> createdServices = paginatedCreatedServicesResult.Services;
            int totalCreatedServicesCount = paginatedCreatedServicesResult.TotalCount;
            int createdServicesTotalPages = (int)Math.Ceiling((double)totalCreatedServicesCount / createdServicesPageSize);

           
            var paginatedReviewsResult = await serviceService.GetReviewsByUserIdAsync(user.Id, reviewsPage, reviewsPageSize);
            IEnumerable<ReviewViewModel> reviews = paginatedReviewsResult.Reviews;
            int totalReviewsCount = paginatedReviewsResult.TotalCount;
            int reviewsTotalPages = (int)Math.Ceiling((double)totalReviewsCount / reviewsPageSize);

            IEnumerable<ServiceViewModel> favoriteServices = await serviceService.GetFavoriteServicesByUserIdAsync(user.Id);
            int approvedServicesCount = await serviceService.GetApprovedServicesCountByUserIdAsync(user.Id);


            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles,
                IsBusiness = user.IsBusiness,
                BusinessExpiresOn = user.BusinessExpiresOn,
                LastServiceCreationDate = user.LastServiceCreationDate,
                ApprovedServicesCount = approvedServicesCount,
                FavoriteServices = favoriteServices, 

                
                CreatedServices = createdServices,
                CreatedServicesCurrentPage = createdServicesPage,
                CreatedServicesPageSize = createdServicesPageSize,
                TotalCreatedServicesCount = totalCreatedServicesCount,
                CreatedServicesTotalPages = createdServicesTotalPages,

              
                Reviews = reviews,
                ReviewsCurrentPage = reviewsPage,
                ReviewsPageSize = reviewsPageSize,
                TotalReviewsCount = totalReviewsCount,
                ReviewsTotalPages = reviewsTotalPages
            };

            return View("~/Areas/Identity/Views/UserProfile.cshtml", viewModel);
        }
    }
}
