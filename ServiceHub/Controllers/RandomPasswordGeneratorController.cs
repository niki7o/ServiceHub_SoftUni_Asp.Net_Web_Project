using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route: /api/RandomPasswordGenerator
    public class RandomPasswordGeneratorController : ControllerBase
    {
        private readonly ILogger<RandomPasswordGeneratorController> _logger;
        private readonly IRandomPasswordGeneratorService _passwordGeneratorService; // Инжектиран сервиз

        public RandomPasswordGeneratorController(
            ILogger<RandomPasswordGeneratorController> logger,
            IRandomPasswordGeneratorService passwordGeneratorService) // Добавен сервиз в конструктора
        {
            _logger = logger;
            _passwordGeneratorService = passwordGeneratorService;
        }

        [HttpPost("generate")] // Full route: /api/RandomPasswordGenerator/generate
        public async Task<IActionResult> GeneratePassword([FromBody] PasswordGenerateRequestModel request) // Направен async
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for PasswordGenerateRequestModel: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            // Делегиране на логиката към сервиза
            var response = await _passwordGeneratorService.GeneratePasswordAsync(request);

            if (!string.IsNullOrEmpty(response.Message) && response.GeneratedPassword == "") 
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response); // Връщане на отговора от сервиза
        }
    }
}
