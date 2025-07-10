using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Common.Enum;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Data.DataSeeder;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Security.Claims;

namespace ServiceHub.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IServiceService serviceService;
        private readonly UserManager<ApplicationUser> userManager;
       private readonly IRepository<Favorite> favoriteRepo;
       private readonly IRepository<Service> serviceRepository;
        private readonly IRepository<Category> _categoryRepository;

        public ServiceController(
            IServiceService serviceService,
            UserManager<ApplicationUser> userManager,
            IRepository<Favorite> favoriteRepo, IRepository<Service> serviceRepository, IRepository<Category> categoryRepository)
        {
            this.serviceService = serviceService;
            this.userManager = userManager;
            this.favoriteRepo = favoriteRepo;
           this.serviceRepository = serviceRepository;
            _categoryRepository = categoryRepository;
        }


        public async Task<IActionResult> All(string? categoryFilter, string? accessTypeFilter)
        {
          
            var allCategories = await _categoryRepository.All().OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(allCategories, "Name", "Name", categoryFilter); 

            var allAccessTypes = Enum.GetNames(typeof(AccessType))
                                     .Select(name => new SelectListItem { Value = name, Text = name })
                                     .ToList();
            ViewBag.AccessTypes = new SelectList(allAccessTypes, "Value", "Text", accessTypeFilter);

            ViewBag.CurrentCategory = categoryFilter;
            ViewBag.CurrentAccessType = accessTypeFilter;

         
            IQueryable<Service> servicesQuery = serviceRepository.All().Include(s => s.Category);

       
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                servicesQuery = servicesQuery.Where(s => s.Category != null && s.Category.Name == categoryFilter);
            }

            if (!string.IsNullOrEmpty(accessTypeFilter))
            {
                if (Enum.TryParse<AccessType>(accessTypeFilter, true, out AccessType parsedAccessType))
                {
                    servicesQuery = servicesQuery.Where(s => s.AccessType == parsedAccessType);
                }
                else
                {
                   
                    // For now, it will simply ignore the invalid filter.
                }
            }

            var services = await servicesQuery.ToListAsync();

            
            var serviceDisplayDtos = services.Select(s => new ServiceSeedModel
            {
                Id = s.Id,
            Title = s.Title,
                Description = s.Description,
                Category = s.Category != null ? s.Category.Name : "N/A",
                AccessType = s.AccessType.ToString(),
               
            }).ToList();

            
            return View(serviceDisplayDtos);
        }





        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var service = await serviceRepository.All()
                                                  .Include(s => s.Category)
                                                  .Include(s => s.Reviews).ThenInclude(r => r.User)
                                                  .Include(s => s.Favorites).ThenInclude(f => f.User)
                                                  .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

           
            bool canUseService = false;
            if (User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(User);
                if (user != null)
                {
                    
                    if (await userManager.IsInRoleAsync(user, "Admin") || await userManager.IsInRoleAsync(user, "BusinessUser"))
                    {
                        canUseService = true;
                    }
                    
                    else if (await userManager.IsInRoleAsync(user, "User"))
                    {
                        if (service.AccessType == Common.Enum.AccessType.Free || service.AccessType == Common.Enum.AccessType.Partial)
                        {
                            canUseService = true;
                        }
                    }
                }
            }
            ViewBag.CanUseService = canUseService;

            return View(service);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(Guid serviceId, ReviewFormModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add review. Please check your input.";
            
                return RedirectToAction(nameof(Details), new { id = serviceId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to leave a review.";
                return RedirectToAction(nameof(Details), new { id = serviceId });
            }

            try
            {
                await serviceService.AddReviewAsync(serviceId, userId, model);
                TempData["SuccessMessage"] = "Review added successfully!";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while adding the review.";
            }

            return RedirectToAction(nameof(Details), new { id = serviceId });
        }






        [Authorize]
        public async Task<IActionResult> UseService(Guid id)
        {
            var service = await serviceRepository.All().FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

           
            if (await userManager.IsInRoleAsync(user, "Admin") || await userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                
                TempData["ServiceMessage"] = $"You are using the service: {service.Title} (Full Access).";
                return RedirectToAction("Details", new { id = service.Id });
            }
          
            else if (await userManager.IsInRoleAsync(user, "User"))
            {
                if (service.AccessType == Common.Enum.AccessType.Free || service.AccessType == Common.Enum.AccessType.Partial)
                {
                    
                    TempData["ServiceMessage"] = $"You are using the service: {service.Title} (Limited Access).";
                    return RedirectToAction("Details", new { id = service.Id });
                }
                else
                {
                    TempData["ServiceMessage"] = $"Access Denied: You need to upgrade to use '{service.Title}'.";
                    return RedirectToAction("Details", new { id = service.Id });
                }
            }
            else
            {
               
                TempData["ServiceMessage"] = "Access Denied: Your role does not permit this action.";
                return RedirectToAction("Details", new { id = service.Id });
            }
        }






































        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(Guid serviceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var existingFavorite = await favoriteRepo.All()
                                           .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceId == serviceId);

            if (existingFavorite != null)
            {
                favoriteRepo.Delete(existingFavorite);
                TempData["SuccessMessage"] = "Removed from favorites!";
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
                TempData["SuccessMessage"] = "Added to favorites!";
            }
            await favoriteRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = serviceId }); 
        }
    }
}
