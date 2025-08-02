using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Core.Models.Service;
using ServiceHub.Core.Models.User;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Security.Claims;

namespace ServiceHub.Areas.Identity.Controllers
{

    [Authorize]
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
        [Route("Profile/{id?}")]
        public async Task<IActionResult> UserProfile(string? id)
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

            IEnumerable<ServiceViewModel> createdServices = await serviceService.GetCreatedServicesByUserIdAsync(user.Id);
            IEnumerable<ServiceViewModel> favoriteServices = await serviceService.GetFavoriteServicesByUserIdAsync(user.Id);
            IEnumerable<ReviewViewModel> reviews = await serviceService.GetReviewsByUserIdAsync(user.Id);
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
                CreatedServices = createdServices,
                FavoriteServices = favoriteServices,
                Reviews = reviews
            };

            return View(viewModel);
        }
    }
    
}
