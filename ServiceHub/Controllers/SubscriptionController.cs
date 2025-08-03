using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models;
using ServiceHub.Data.Models;
using System.Security.Claims;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager; 
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager, 
            ILogger<SubscriptionController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
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

            if (!await _roleManager.RoleExistsAsync("BusinessUser"))
            {
                _logger.LogError("Role 'BusinessUser' does not exist. Please seed roles.");
                return StatusCode(500, new { message = "Възникна вътрешна грешка: Ролята 'BusinessUser' не е намерена." });
            }

           
            if (await _userManager.IsInRoleAsync(user, "BusinessUser"))
            {
                _logger.LogInformation("User {UserName} ({UserId}) is already a BusinessUser. Attempting to renew subscription.", user.UserName, userId);
                user.IsBusiness = true;
                user.BusinessExpiresOn = DateTime.UtcNow.AddDays(30);
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to update BusinessExpiresOn for user {UserName}: {Errors}", user.UserName, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    return StatusCode(500, new { message = "Неуспешно подновяване на абонамента. Моля, опитайте отново." });
                }

                await _signInManager.RefreshSignInAsync(user);

                _logger.LogInformation("User {UserName} ({UserId}) Business subscription renewed until {ExpiryDate}.", user.UserName, userId, user.BusinessExpiresOn);
                return Ok(new { message = "Абонаментът Ви за Бизнес Потребител е успешно подновен за 30 дни!", expiresOn = user.BusinessExpiresOn?.ToString("yyyy-MM-dd") });
            }

            _logger.LogInformation("Simulating successful payment for user {UserName} ({UserId}).", user.UserName, userId);

         
            if (await _userManager.IsInRoleAsync(user, "User"))
            {
                var removeUserRoleResult = await _userManager.RemoveFromRoleAsync(user, "User");
                if (!removeUserRoleResult.Succeeded)
                {
                    _logger.LogError("Failed to remove 'User' role from {UserName}: {Errors}", user.UserName, string.Join(", ", removeUserRoleResult.Errors.Select(e => e.Description)));
                    
                }
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "BusinessUser");
            if (!addRoleResult.Succeeded)
            {
                _logger.LogError("Failed to add 'BusinessUser' role to {UserName}: {Errors}", user.UserName, string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                return StatusCode(500, new { message = "Неуспешно активиране на абонамента. Моля, свържете се с поддръжката." });
            }

            user.IsBusiness = true;
            user.BusinessExpiresOn = DateTime.UtcNow.AddDays(30);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user {UserName} properties after subscription: {Errors}", user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                
                await _userManager.RemoveFromRoleAsync(user, "BusinessUser");
             
                if (!await _userManager.IsInRoleAsync(user, "User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                return StatusCode(500, new { message = "Неуспешно активиране на абонамента. Моля, опитайте отново." });
            }

         
            await _signInManager.RefreshSignInAsync(user);

            _logger.LogInformation("User {UserName} ({UserId}) successfully subscribed as BusinessUser until {ExpiryDate}.", user.UserName, userId, user.BusinessExpiresOn);
            return Ok(new { message = "Поздравления! Вече сте Бизнес Потребител за 30 дни!", expiresOn = user.BusinessExpiresOn?.ToString("yyyy-MM-dd") });
        }
    }
}
