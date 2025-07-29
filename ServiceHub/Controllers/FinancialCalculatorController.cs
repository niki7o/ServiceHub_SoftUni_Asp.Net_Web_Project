using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
   [Authorize]
    public class FinancialCalculatorController : Controller
    {
        private readonly IFinancialCalculatorService _financialCalculatorService;
        private readonly ILogger<FinancialCalculatorController> _logger;

        
        public static readonly Guid FinancialCalculatorId = Guid.Parse("2EF43D87-D749-4D7D-9B7D-F7C4F527BEA7");

        public FinancialCalculatorController(IFinancialCalculatorService financialCalculatorService, ILogger<FinancialCalculatorController> logger)
        {
            _financialCalculatorService = financialCalculatorService;
            _logger = logger;
        }

       
        [HttpGet("/FinancialCalculator/CalculatorForm")]
        public IActionResult CalculatorForm()
        {
            _logger.LogInformation("Serving FinancialCalculatorForm view.");
           
            ViewData["ServiceId"] = FinancialCalculatorId.ToString();
            return View("~/Views/Service/_FinancialCalculatorForm.cshtml");
        }

       
        [HttpPost("/api/FinancialCalculator/calculate")]
        public async Task<IActionResult> CalculateFinancials([FromBody] FinancialCalculatorRequestModel request)
        {
            _logger.LogDebug("Incoming FinancialCalculatorRequestModel: {@Request}", request);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();
                _logger.LogWarning("Model validation failed for FinancialCalculatorRequestModel: {Errors}", string.Join("; ", errors));
                return BadRequest(new { errors = errors, message = "Въведените данни са невалидни. Моля, проверете всички полета." });
            }

            _logger.LogInformation("Received financial calculation request.");

            var result = await _financialCalculatorService.CalculateFinancialsAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Financial report successfully generated. File: {FileName}", result.GeneratedFileName);
                return File(result.GeneratedFileContent, result.ContentType, result.GeneratedFileName);
            }
            else
            {
                _logger.LogError("Failed to generate financial report. Error: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage ?? "Неизвестна грешка при генериране на финансов отчет.");
            }
        }
    }
}
