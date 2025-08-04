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
        private readonly ILogger<ReviewController> _logger; 

        public ReviewController(
            IReviewService reviewService,
            IServiceService serviceService,
            UserManager<ApplicationUser> userManager,
            ILogger<ReviewController> logger) 
        {
            _reviewService = reviewService;
            _serviceService = serviceService;
            _userManager = userManager;
            _logger = logger; 
        }

        [HttpGet]
        public async Task<IActionResult> CreateReview(Guid serviceId) 
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var service = await _serviceService.GetByIdAsync(serviceId, currentUserId);

                if (service == null)
                {
                    _logger.LogWarning($"AddReview GET: Service with ID {serviceId} not found.");
                    TempData["ErrorMessage"] = "Услугата не е намерена.";
                    return RedirectToAction("All", "Service");
                }
              
                if (service.IsTemplate && !service.IsApproved)
                {
                    _logger.LogWarning($"AddReview GET: Attempt to add review for unapproved template or template service {serviceId}.");
                    TempData["ErrorMessage"] = "Не може да добавяте ревюта за шаблони или неодобрени услуги.";
                    return RedirectToAction("Details", "Service", new { id = serviceId });
                }

                ViewBag.ServiceId = serviceId;
                ViewBag.ServiceTitle = service.Title;

                return View(new ReviewFormModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddReview GET: Unexpected error for service {ServiceId} by user {UserId}", serviceId, currentUserId);
                TempData["ErrorMessage"] = "Възникна грешка при зареждане на формата за ревю.";
                return RedirectToAction("Details", "Service", new { id = serviceId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(Guid serviceId, ReviewFormModel model)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid)
            {
                ViewBag.ServiceId = serviceId;
                try
                {
                    var service = await _serviceService.GetByIdAsync(serviceId, userId);
                    ViewBag.ServiceTitle = service?.Title ?? "Неизвестна услуга";
                }
                catch { ViewBag.ServiceTitle = "Неизвестна услуга"; } 
                TempData["ErrorMessage"] = "Моля, попълнете всички задължителни полета коректно.";
                return View(model);
            }

            if (userId == null)
            {
                _logger.LogWarning("AddReview POST: Unauthenticated user attempted to add a review.");
                TempData["ErrorMessage"] = "Трябва да сте логнати, за да оставите ревю.";
                return Unauthorized();
            }

            try
            {
                await _reviewService.AddReviewAsync(serviceId, userId, model);
                TempData["SuccessMessage"] = "Ревюто е успешно добавено!";
                return RedirectToAction("Details", "Service", new { id = serviceId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "AddReview POST: {Message} for service {ServiceId} by user {UserId}", ex.Message, serviceId, userId);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "AddReview POST: {Message} for service {ServiceId} by user {UserId}", ex.Message, serviceId, userId);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddReview POST: Unexpected error adding review for service {ServiceId} by user {UserId}", serviceId, userId);
                TempData["ErrorMessage"] = "Възникна неочаквана грешка при добавяне на ревюто.";
            }

            return RedirectToAction("Details", "Service", new { id = serviceId });
        }

        [HttpGet]
        public async Task<IActionResult> EditReview(Guid reviewId, Guid serviceId) 
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                TempData["ErrorMessage"] = "Трябва да сте логнати, за да редактирате ревю.";
                return RedirectToAction("Details", "Service", new { id = serviceId });
            }

            bool isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Admin");

            try
            {
                var model = await _reviewService.GetReviewForEditAsync(reviewId, currentUserId, isAdmin);

                var reviewEntity = await _reviewService.GetReviewByIdInternal(reviewId);
                if (reviewEntity == null)
                {
                    _logger.LogWarning($"EditReview GET: Review with ID {reviewId} not found for associated service.");
                    TempData["ErrorMessage"] = "Ревюто не е намерено.";
                    return RedirectToAction("Details", "Service", new { id = serviceId });
                }

                var service = await _serviceService.GetByIdAsync(reviewEntity.ServiceId, currentUserId);
                if (service == null)
                {
                    _logger.LogWarning($"EditReview GET: Associated service with ID {reviewEntity.ServiceId} not found for review {reviewId}.");
                    TempData["ErrorMessage"] = "Свързаната услуга не е намерена.";
                    return RedirectToAction("All", "Service");
                }

                ViewBag.ReviewId = reviewId;
                ViewBag.ServiceId = service.Id;
                ViewBag.ServiceTitle = service.Title;

                return View(model);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "EditReview GET: {Message} for review {ReviewId} by user {UserId}", ex.Message, reviewId, currentUserId);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Service", new { id = serviceId });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "EditReview GET: {Message} for review {ReviewId} by user {UserId}", ex.Message, reviewId, currentUserId);
                TempData["ErrorMessage"] = ex.Message;
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditReview GET: Unexpected error for review {ReviewId} by user {UserId}", reviewId, currentUserId);
                TempData["ErrorMessage"] = "Възникна неочаквана грешка при зареждане на ревюто за редактиране.";
                return RedirectToAction("Details", "Service", new { id = serviceId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(Guid reviewId, ReviewFormModel model)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                _logger.LogWarning("EditReview POST: Unauthenticated user attempted to edit a review.");
                TempData["ErrorMessage"] = "Трябва да сте логнати, за да редактирате ревю.";
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
                TempData["ErrorMessage"] = "Моля, попълнете всички задължителни полета коректно.";
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
                TempData["SuccessMessage"] = "Ревюто е успешно обновено!";
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "EditReview POST: {Message} for review {ReviewId} by user {UserId}", ex.Message, reviewId, currentUserId);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "EditReview POST: {Message} for review {ReviewId} by user {UserId}", ex.Message, reviewId, currentUserId);
                TempData["ErrorMessage"] = ex.Message;
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditReview POST: Unexpected error updating review {ReviewId} by user {UserId}", reviewId, currentUserId);
                TempData["ErrorMessage"] = "Възникна неочаквана грешка при обновяване на ревюто.";
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
                _logger.LogWarning("DeleteReview POST: Unauthenticated user attempted to delete a review.");
                TempData["ErrorMessage"] = "Трябва да сте логнати, за да изтриете ревю.";
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
                TempData["SuccessMessage"] = "Ревюто е успешно изтрито.";
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "DeleteReview POST: {Message} for review {ReviewId}. Reason: {Message}", ex.Message, reviewId, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "DeleteReview POST: Unauthorized attempt to delete review {ReviewId} by user {CurrentUserId}. Reason: {Message}", reviewId, currentUserId, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteReview POST: Unexpected error deleting review {ReviewId}.", reviewId);
                TempData["ErrorMessage"] = "Възникна неочаквана грешка при изтриване на ревюто.";
            }

            if (serviceIdToRedirect.HasValue)
            {
                return RedirectToAction("Details", "Service", new { id = serviceIdToRedirect.Value });
            }
            return RedirectToAction("All", "Service");
        }
    }

}
