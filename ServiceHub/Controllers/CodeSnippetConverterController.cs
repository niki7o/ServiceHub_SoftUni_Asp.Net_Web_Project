using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CodeSnippetConverterController : ControllerBase
    {
        private readonly ICodeSnippetConverterService _codeSnippetConverterService;
        private readonly ILogger<CodeSnippetConverterController> _logger;
        private readonly UserManager<ApplicationUser> _userManager; 

        public CodeSnippetConverterController(
            ICodeSnippetConverterService codeSnippetConverterService,
            ILogger<CodeSnippetConverterController> logger,
            UserManager<ApplicationUser> userManager) 
        {
            _codeSnippetConverterService = codeSnippetConverterService;
            _logger = logger;
            _userManager = userManager; 
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertCode([FromBody] CodeSnippetConvertRequestModel request)
        {
            _logger.LogInformation("Received code conversion request in API controller.");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                _logger.LogWarning("Model validation failed for CodeSnippetConvertRequestModel: {Errors}", string.Join("; ", errors));
                return BadRequest(new { errors = errors, message = "Въведените данни са невалидни. Моля, проверете всички полета." });
            }

            try
            {
              
                var user = await _userManager.GetUserAsync(User);
                bool isBusinessUser = user != null && await _userManager.IsInRoleAsync(user, "BusinessUser");

               
                var response = await _codeSnippetConverterService.ConvertCodeAsync(request, isBusinessUser);

                
                if (response.Message != null && response.Message.Contains("Достъпът до JavaScript и PHP конвертиране е само за Бизнес Потребители") && !isBusinessUser)
                {
                    return Forbid(response.Message);
                }

                _logger.LogInformation("Code conversion successful via service.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during code conversion in API controller.");
                return StatusCode(500, new { message = "Възникна грешка при конвертиране на кода: " + ex.Message });
            }
        
    }

    }
}