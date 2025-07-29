using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class RandomPasswordGeneratorController : ControllerBase
    {
        private readonly ILogger<RandomPasswordGeneratorController> _logger;
        private readonly IRandomPasswordGeneratorService _passwordGeneratorService; 

        public RandomPasswordGeneratorController(
            ILogger<RandomPasswordGeneratorController> logger,
            IRandomPasswordGeneratorService passwordGeneratorService)
        {
            _logger = logger;
            _passwordGeneratorService = passwordGeneratorService;
        }

        [HttpPost("generate")] 
        public async Task<IActionResult> GeneratePassword([FromBody] PasswordGenerateRequestModel request) 
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for PasswordGenerateRequestModel: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var response = await _passwordGeneratorService.GeneratePasswordAsync(request);

            if (!string.IsNullOrEmpty(response.Message) && response.GeneratedPassword == "") 
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response); 
        }
    }
}
