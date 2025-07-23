using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models;
using ServiceHub.Data.Models;
using System.Security.Claims;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route: /api/Subscription
    [Authorize] // Only for logged-in users
    public class SubscriptionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<SubscriptionController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for SubscribeRequestModel: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            if (!request.ConfirmSubscription)
            {
                return BadRequest(new { message = "Моля, потвърдете абонамента си." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized attempt to subscribe: User ID not found in claims.");
                return Unauthorized(new { message = "Неупълномощен достъп. Моля, влезте в профила си." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User with ID {UserId} not found during subscription attempt.", userId);
                return NotFound(new { message = "Потребителят не е намерен." });
            }

            // Check if "BusinessUser" role exists
            if (!await _roleManager.RoleExistsAsync("BusinessUser"))
            {
                _logger.LogError("Role 'BusinessUser' does not exist. Please seed roles.");
                return StatusCode(500, new { message = "Възникна вътрешна грешка: Ролята 'BusinessUser' не е намерена." });
            }

            // Check if user is already a BusinessUser
            if (await _userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                // If already a business user, it's a renewal
                _logger.LogInformation("User {UserName} ({UserId}) is already a BusinessUser. Attempting to renew subscription.", user.UserName, userId);
                user.IsBusiness = true;
                user.BusinessExpiresOn = DateTime.UtcNow.AddDays(30); // Renew for another 30 days
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to update BusinessExpiresOn for user {UserName}: {Errors}", user.UserName, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    return StatusCode(500, new { message = "Неуспешно подновяване на абонамента. Моля, опитайте отново." });
                }
                _logger.LogInformation("User {UserName} ({UserId}) Business subscription renewed until {ExpiryDate}.", user.UserName, userId, user.BusinessExpiresOn);
                return Ok(new { message = "Абонаментът Ви за Бизнес Потребител е успешно подновен за 30 дни!", expiresOn = user.BusinessExpiresOn?.ToString("yyyy-MM-dd") });
            }

            // Simulate successful transaction
            _logger.LogInformation("Simulating successful payment for user {UserName} ({UserId}).", user.UserName, userId);

            // 1. Remove "User" role, if exists
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                var removeUserRoleResult = await _userManager.RemoveFromRoleAsync(user, "User");
                if (!removeUserRoleResult.Succeeded)
                {
                    _logger.LogError("Failed to remove 'User' role from {UserName}: {Errors}", user.UserName, string.Join(", ", removeUserRoleResult.Errors.Select(e => e.Description)));
                    // Continue, but log the error
                }
            }

            // 2. Add "BusinessUser" role
            var addRoleResult = await _userManager.AddToRoleAsync(user, "BusinessUser");
            if (!addRoleResult.Succeeded)
            {
                _logger.LogError("Failed to add 'BusinessUser' role to {UserName}: {Errors}", user.UserName, string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                return StatusCode(500, new { message = "Неуспешно активиране на абонамента. Моля, свържете се с поддръжката." });
            }

            // 3. Update user data
            user.IsBusiness = true;
            user.BusinessExpiresOn = DateTime.UtcNow.AddDays(30); // Subscription for 30 days
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserName} properties after subscription: {Errors}", user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                // If user update fails, try to remove the role if it was added
                await _userManager.RemoveFromRoleAsync(user, "BusinessUser");
                return StatusCode(500, new { message = "Неуспешно активиране на абонамента. Моля, опитайте отново." });
            }

            _logger.LogInformation("User {UserName} ({UserId}) successfully subscribed as BusinessUser until {ExpiryDate}.", user.UserName, userId, user.BusinessExpiresOn);
            return Ok(new { message = "Поздравления! Вече сте Бизнес Потребител за 30 дни!", expiresOn = user.BusinessExpiresOn?.ToString("yyyy-MM-dd") });
        }
    }
}
