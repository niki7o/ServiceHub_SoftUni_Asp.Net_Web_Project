using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Reviews;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Security.Claims;

namespace ServiceHub.Controllers
{

  

        [Authorize] 
        public class ReviewController : Controller
        {
            private readonly IReviewService _reviewService;
            private readonly IServiceService _serviceService;
            private readonly UserManager<ApplicationUser> _userManager; 

            public ReviewController(
                IReviewService reviewService,
                IServiceService serviceService,
                UserManager<ApplicationUser> userManager)
            {
                _reviewService = reviewService;
                _serviceService = serviceService;
                _userManager = userManager;
            }

            [HttpGet]
            public async Task<IActionResult> CreateReview(Guid serviceId)
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var service = await _serviceService.GetByIdAsync(serviceId, currentUserId);

                if (service == null)
                {
                    TempData["ErrorMessage"] = "Service not found.";
                    return RedirectToAction("All", "Service");
                }

                ViewBag.ServiceId = serviceId;
                ViewBag.ServiceTitle = service.Title;

                return View(new ReviewFormModel());
            }

            
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddReview(Guid serviceId, ReviewFormModel model)
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Failed to add review. Please check your input.";
                    return RedirectToAction(nameof(CreateReview), new { serviceId = serviceId });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) 
                {
                    TempData["ErrorMessage"] = "You must be logged in to leave a review.";
                    return Unauthorized();
                }

                try
                {
                    await _reviewService.AddReviewAsync(serviceId, userId, model);
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

                return RedirectToAction("Details", "Service", new { id = serviceId });
            }

            [HttpGet]
            public async Task<IActionResult> EditReview(Guid reviewId)
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == null)
                {
                    return Unauthorized();
                }

                bool isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Admin");

                try
                {
                    var model = await _reviewService.GetReviewForEditAsync(reviewId, currentUserId, isAdmin);

                    var reviewEntity = await _reviewService.GetReviewByIdInternal(reviewId);
                    if (reviewEntity == null)
                    {
                        TempData["ErrorMessage"] = "Review not found for associated service.";
                        return RedirectToAction("All", "Service");
                    }

                    var service = await _serviceService.GetByIdAsync(reviewEntity.ServiceId, currentUserId);
                    if (service == null)
                    {
                        TempData["ErrorMessage"] = "Associated service not found.";
                        return RedirectToAction("All", "Service");
                    }

                    ViewBag.ReviewId = reviewId;
                    ViewBag.ServiceId = service.Id;
                    ViewBag.ServiceTitle = service.Title;

                    return View(model);
                }
                catch (ArgumentException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return NotFound();
                }
                catch (UnauthorizedAccessException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return Forbid();
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while loading the review for editing.";
                    return RedirectToAction("All", "Service");
                }
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> EditReview(Guid reviewId, ReviewFormModel model)
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == null)
                {
                    return Unauthorized();
                }

                bool isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Admin");

                Guid? serviceIdToRedirect = null;
                var reviewToUpdate = await _reviewService.GetReviewByIdInternal(reviewId);
                if (reviewToUpdate != null)
                {
                    serviceIdToRedirect = reviewToUpdate.ServiceId;
                }

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Failed to update review. Please check your input.";
                    if (serviceIdToRedirect.HasValue)
                    {
                        var service = await _serviceService.GetByIdAsync(serviceIdToRedirect.Value, currentUserId);
                        if (service != null)
                        {
                            ViewBag.ServiceId = service.Id;
                            ViewBag.ServiceTitle = service.Title;
                        }
                    }
                    ViewBag.ReviewId = reviewId;
                    return View(model);
                }

                try
                {
                    await _reviewService.UpdateReviewAsync(reviewId, currentUserId, model, isAdmin);
                    TempData["SuccessMessage"] = "Review updated successfully!";
                }
                catch (ArgumentException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (UnauthorizedAccessException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return Forbid();
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while updating the review.";
                }

                if (serviceIdToRedirect.HasValue)
                {
                    return RedirectToAction("Details", "Service", new { id = serviceIdToRedirect.Value });
                }
                return RedirectToAction("All", "Service");
            }

      
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteReview(Guid reviewId)
            {
                string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId == null)
                {
                    return Unauthorized();
                }

                bool isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Admin");

                Guid? serviceIdToRedirect = null;
                var reviewToDelete = await _reviewService.GetReviewByIdInternal(reviewId);
                if (reviewToDelete != null)
                {
                    serviceIdToRedirect = reviewToDelete.ServiceId;
                }

                try
                {
                    await _reviewService.DeleteReviewAsync(reviewId, currentUserId, isAdmin);
                    TempData["SuccessMessage"] = "Review deleted successfully!";
                }
                catch (ArgumentException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (UnauthorizedAccessException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return Forbid();
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while deleting the review.";
                }

                if (serviceIdToRedirect.HasValue)
                {
                    return RedirectToAction("Details", "Service", new { id = serviceIdToRedirect.Value });
                }
                return RedirectToAction("All", "Service");
            }
        }

    }
