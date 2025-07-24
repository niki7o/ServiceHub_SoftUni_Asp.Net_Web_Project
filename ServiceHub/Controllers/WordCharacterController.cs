using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WordCharacterController : ControllerBase
    {
        private readonly ILogger<WordCharacterController> _logger;
        private readonly IWordCharacterCounterService _wordCharacterCounterService;

        public WordCharacterController(
            ILogger<WordCharacterController> logger,
            IWordCharacterCounterService wordCharacterCounterService)
        {
            _logger = logger;
            _wordCharacterCounterService = wordCharacterCounterService;
        }

        [HttpPost("count")]
        public async Task<IActionResult> CountText([FromBody] WordCharacterCountRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for WordCharacterCountRequestModel: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var response = await _wordCharacterCounterService.CountTextAsync(request);

            return Ok(response);
        }
    }
}
