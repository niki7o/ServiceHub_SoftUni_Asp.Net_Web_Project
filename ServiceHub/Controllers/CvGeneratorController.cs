using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{

    [Authorize]
    public class CvGeneratorController : Controller
    {
        private readonly ICvGeneratorService _cvGeneratorService;
        private readonly ILogger<CvGeneratorController> _logger;

        public CvGeneratorController(ICvGeneratorService cvGeneratorService, ILogger<CvGeneratorController> logger)
        {
            _cvGeneratorService = cvGeneratorService;
            _logger = logger;
        }

        [HttpGet("/CvGenerator/CvGeneratorForm")] // Абсолютен маршрут за View-то
        public IActionResult CvGeneratorForm()
        {
            _logger.LogInformation("Serving CvGeneratorForm view.");
            return View("~/Views/Service/_CvGeneratorForm.cshtml");
        }

        [HttpPost("/api/CvGenerator/generate")] // Пълен API маршрут
        public async Task<IActionResult> GenerateCv([FromBody] CvGenerateRequestModel request)
        {
            _logger.LogDebug("Incoming CvGenerateRequestModel: {@Request}", request);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                _logger.LogWarning("Model validation failed for CvGenerateRequestModel: {Errors}", string.Join("; ", errors));
                return BadRequest(new { errors = errors, message = "Въведените данни са невалидни. Моля, проверете всички полета." });
            }

            _logger.LogInformation("Received CV generation request for {Name}.", request.Name);

            var result = await _cvGeneratorService.GenerateCvAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("CV successfully generated for {Name}. File: {FileName}", request.Name, result.GeneratedFileName);
                return File(result.GeneratedFileContent, result.ContentType, result.GeneratedFileName);
            }
            else
            {
                _logger.LogError("Failed to generate CV for {Name}. Error: {ErrorMessage}", request.Name, result.ErrorMessage);
                return BadRequest(result.ErrorMessage ?? "Неизвестна грешка при генериране на CV.");
            }
        }
    }
}
