using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Areas.Admin.Models;
using ServiceHub.Areas.Admin.Services.Interface;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;

using System.Security.Claims;
using System.Threading.Tasks;

namespace ServiceHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IServiceService _serviceService;
        private readonly IUserService _userService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger,
            IServiceService serviceService,
            IUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _serviceService = serviceService;
            _userService = userService;
        }

        public async Task<IActionResult> AllUsers(int usersPage = 1, int pendingTemplatesPage = 1)
        {
          
            int usersPageSize = 5;
            int pendingTemplatesPageSize = 5;

        
            var paginatedUsersResult = await _userService.GetAllUsersAsync(usersPage, usersPageSize);

            var paginatedPendingTemplatesResult = await _serviceService.GetAllPendingTemplatesAsync(pendingTemplatesPage, pendingTemplatesPageSize);

            var viewModel = new AdminDashboardViewModel
            {
                
                Users = paginatedUsersResult.Users,
                UsersCurrentPage = usersPage,
                UsersPageSize = usersPageSize,
                TotalUsersCount = paginatedUsersResult.TotalCount,
                UsersTotalPages = (int)Math.Ceiling((double)paginatedUsersResult.TotalCount / usersPageSize),

                PendingServices = paginatedPendingTemplatesResult.Templates,
                PendingServicesCurrentPage = pendingTemplatesPage,
                PendingServicesPageSize = pendingTemplatesPageSize,
                TotalPendingServicesCount = paginatedPendingTemplatesResult.TotalCount,
                PendingServicesTotalPages = (int)Math.Ceiling((double)paginatedPendingTemplatesResult.TotalCount / pendingTemplatesPageSize)
            };

            return View(viewModel);
        }

     
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteBusinessUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Невалиден потребителски идентификатор.";
                return RedirectToAction(nameof(AllUsers));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Потребителят не е намерен.";
                return RedirectToAction(nameof(AllUsers));
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "Не можете да демоутнете собствения си акаунт.";
                return RedirectToAction(nameof(AllUsers));
            }

            if (await _userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, "BusinessUser");
                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = $"Грешка при демоутване на потребителя: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}";
                    _logger.LogError($"Failed to remove 'BusinessUser' role from user {user.Id}: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                    return RedirectToAction(nameof(AllUsers));
                }

                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }
                    var addResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!addResult.Succeeded)
                    {
                        _logger.LogError($"Failed to add 'User' role to user {user.Id}: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                        TempData["WarningMessage"] = "Потребителят беше демоутнат, но възникна проблем при добавяне на ролята 'User'.";
                    }
                }

                TempData["SuccessMessage"] = $"Потребител '{user.UserName}' успешно демоутнат до User.";
            }
            else
            {
                TempData["WarningMessage"] = "Потребителят не е BusinessUser и не може да бъде демоутнат до User.";
            }

            return RedirectToAction(nameof(AllUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToBusinessUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Невалиден потребителски идентификатор.";
                return RedirectToAction(nameof(AllUsers));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Потребителят не е намерен.";
                return RedirectToAction(nameof(AllUsers));
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "Не можете да промоутнете собствения си акаунт.";
                return RedirectToAction(nameof(AllUsers));
            }

            if (await _userManager.IsInRoleAsync(user, "BusinessUser") || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["WarningMessage"] = "Потребителят вече е BusinessUser или Admin и не може да бъде промоутнат.";
                return RedirectToAction(nameof(AllUsers));
            }

            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                var removeNormalUserResult = await _userManager.RemoveFromRoleAsync(user, "User");
                if (!removeNormalUserResult.Succeeded)
                {
                    _logger.LogError($"Failed to remove 'User' role from user {user.Id} during promotion: {string.Join(", ", removeNormalUserResult.Errors.Select(e => e.Description))}");
                    TempData["ErrorMessage"] = "Възникна проблем при премахване на ролята 'User'.";
                    return RedirectToAction(nameof(AllUsers));
                }
            }

            if (!await _roleManager.RoleExistsAsync("BusinessUser"))
            {
                await _roleManager.CreateAsync(new IdentityRole("BusinessUser"));
            }
            var addResult = await _userManager.AddToRoleAsync(user, "BusinessUser");
            if (!addResult.Succeeded)
            {
                TempData["ErrorMessage"] = $"Грешка при промоутване на потребителя: {string.Join(", ", addResult.Errors.Select(e => e.Description))}";
                _logger.LogError($"Failed to add 'BusinessUser' role to user {user.Id}: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                return RedirectToAction(nameof(AllUsers));
            }

            TempData["SuccessMessage"] = $"Потребител '{user.UserName}' успешно промоутнат до BusinessUser.";
            return RedirectToAction(nameof(AllUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Невалиден потребителски идентификатор.";
                return RedirectToAction(nameof(AllUsers));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Потребителят не е намерен.";
                return RedirectToAction(nameof(AllUsers));
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "Не можете да изтриете собствения си акаунт.";
                return RedirectToAction(nameof(AllUsers));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                TempData["ErrorMessage"] = "Не можете да изтриете Администратор или BusinessUser директно. Моля, първо понижете ролята им.";
                return RedirectToAction(nameof(AllUsers));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Потребител '{user.UserName}' успешно изтрит.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Грешка при изтриване на потребител: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(AllUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveServiceTemplate(Guid id)
        {
            string? adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId == null)
            {
                TempData["ErrorMessage"] = "Неупълномощен достъп.";
                return Unauthorized();
            }

            try
            {
                await _serviceService.ApproveServiceTemplateAsync(id, adminId);
                TempData["SuccessMessage"] = "Шаблонът за услуга е успешно одобрен и публикуван!";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving service template ID: {id}.");
                TempData["ErrorMessage"] = "Възникна грешка при одобряване на шаблона.";
            }

            return RedirectToAction(nameof(AllUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectServiceTemplate(Guid id)
        {
            string? adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId == null)
            {
                TempData["ErrorMessage"] = "Неупълномощен достъп.";
                return Unauthorized();
            }

            try
            {
                await _serviceService.RejectServiceTemplateAsync(id, adminId);
                TempData["SuccessMessage"] = "Шаблонът за услуга е успешно отхвърлен и изтрит!";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting service template ID: {id}.");
                TempData["ErrorMessage"] = "Възникна грешка при отхвърляне на шаблона.";
            }

            return RedirectToAction(nameof(AllUsers));
        }
    }
}
