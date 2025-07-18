using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceHub.Core.Models.Users;
using ServiceHub.Data.Models;

namespace ServiceHub.Controllers
{

    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

   
        public async Task<IActionResult> AllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return View(userViewModels);
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

            // Защита: Админ не може да демоутва себе си
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "Не можете да демоутнете собствения си акаунт.";
                return RedirectToAction(nameof(AllUsers));
            }

            // Проверяваме дали потребителят е BusinessUser
            if (await _userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, "BusinessUser");
                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = $"Грешка при демоутване на потребителя: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}";
                    _logger.LogError($"Failed to remove 'BusinessUser' role from user {user.Id}: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                    return RedirectToAction(nameof(AllUsers));
                }

                // Добавяме ролята "NormalUser", ако не я притежава вече (или ако е била премахната)
                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    var addResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!addResult.Succeeded)
                    {
                        _logger.LogError($"Failed to add 'NormalUser' role to user {user.Id}: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                        TempData["WarningMessage"] = "Потребителят беше демоутнат, но възникна проблем при добавяне на ролята 'NormalUser'.";
                    }
                }

                TempData["SuccessMessage"] = $"Потребител '{user.UserName}' успешно демоутнат до NormalUser.";
            }
            else
            {
                TempData["WarningMessage"] = "Потребителят не е Business User и не може да бъде демоутнат.";
            }

            return RedirectToAction(nameof(AllUsers));
        }

        [HttpPost] // Тази операция трябва да е POST
        [ValidateAntiForgeryToken] // Защита от CSRF атаки
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

            // Защита: Админ не може да промоутва себе си
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "Не можете да промоутнете собствения си акаунт.";
                return RedirectToAction(nameof(AllUsers));
            }

            // Проверяваме дали потребителят вече не е BusinessUser или Admin
            if (await _userManager.IsInRoleAsync(user, "BusinessUser") || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["WarningMessage"] = "Потребителят вече е Business User или Admin и не може да бъде промоутнат.";
                return RedirectToAction(nameof(AllUsers));
            }

            // Премахваме ролята "NormalUser", ако я притежава
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                var removeNormalUserResult = await _userManager.RemoveFromRoleAsync(user, "User");
                if (!removeNormalUserResult.Succeeded)
                {
                    _logger.LogError($"Failed to remove 'NormalUser' role from user {user.Id} during promotion: {string.Join(", ", removeNormalUserResult.Errors.Select(e => e.Description))}");
                    TempData["ErrorMessage"] = "Възникна проблем при премахване на ролята 'NormalUser'.";
                    return RedirectToAction(nameof(AllUsers));
                }
            }

            // Добавяме ролята "BusinessUser"
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

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["ErrorMessage"] = "Не можете да изтриете друг администратор директно.";
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
    }

}
