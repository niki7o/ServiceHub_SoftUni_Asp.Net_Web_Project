using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TextCaseConverterController : ControllerBase
    {
        private readonly ILogger<TextCaseConverterController> _logger;
        private readonly ITextCaseConverterService _textCaseConverterService;

        public TextCaseConverterController(
            ILogger<TextCaseConverterController> logger,
            ITextCaseConverterService textCaseConverterService)
        {
            _logger = logger;
            _textCaseConverterService = textCaseConverterService;
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertCase([FromBody] TextCaseConvertRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for TextCaseConvertRequestModel: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var response = await _textCaseConverterService.ConvertCaseAsync(request);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(new { message = response.Message ?? "Възникна грешка при конвертиране на текста." });
            }
        }

    }
}
