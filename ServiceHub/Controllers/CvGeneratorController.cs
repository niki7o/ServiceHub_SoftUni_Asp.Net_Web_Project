using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [Route("api/[controller]")] // Дефинира базовия маршрут за API действията (e.g., /api/CvGenerator)
    [Authorize] // Може да решите дали да изисквате оторизация тук или на ниво ServiceController
    public class CvGeneratorController : Controller // КОРЕКЦИЯ: Наследява Controller
    {
        private readonly ICvGeneratorService _cvGeneratorService;
        private readonly ILogger<CvGeneratorController> _logger;

        public CvGeneratorController(ICvGeneratorService cvGeneratorService, ILogger<CvGeneratorController> logger)
        {
            _cvGeneratorService = cvGeneratorService;
            _logger = logger;
        }

        // Екшън за показване на формата за генериране на CV
        // Този маршрут е за директен достъп до формата.
        [HttpGet("/Service/UseService/CvGeneratorForm")]
        public IActionResult CvGeneratorForm()
        {
            // Този екшън просто връща View-то на формата.
            // ServiceId вече не е нужен във ViewBag, тъй като формата ще изпраща директно към този контролер.
            _logger.LogInformation("Serving CvGeneratorForm view.");
            return View("~/Views/Service/_CvGeneratorForm.cshtml");
        }


        // Екшън за генериране на CV (API endpoint)
        [HttpPost("generate")] // Пълен маршрут: /api/CvGenerator/generate
        public async Task<IActionResult> GenerateCv([FromBody] CvGenerateRequestModel request)
        {
            // NEW: Логваме входящата заявка за дебъгване
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

            // Изпълняваме услугата директно чрез CvGeneratorService
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
