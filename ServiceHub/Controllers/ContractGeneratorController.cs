using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [Authorize]
    public class ContractGeneratorController : Controller
    {
        private readonly IContractGeneratorService _contractGeneratorService;
        private readonly ILogger<ContractGeneratorController> _logger;

        public ContractGeneratorController(IContractGeneratorService contractGeneratorService, ILogger<ContractGeneratorController> logger)
        {
            _contractGeneratorService = contractGeneratorService;
            _logger = logger;
        }

    
        [HttpGet("/ContractGenerator/ContractGeneratorForm")]
        public IActionResult ContractGeneratorForm()
        {
            _logger.LogInformation("Serving ContractGeneratorForm view.");
            return View("~/Views/Service/_ContractGeneratorForm.cshtml");
        }

      
        [HttpPost("/api/ContractGenerator/generate")]
        public async Task<IActionResult> GenerateContract([FromBody] ContractGenerateRequestModel request)
        {
            _logger.LogDebug("Incoming ContractGenerateRequestModel: {@Request}", request);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                            .Select(e => e.ErrorMessage)
                                            .ToList();
                _logger.LogWarning("Model validation failed for ContractGenerateRequestModel: {Errors}", string.Join("; ", errors));
                return BadRequest(new { errors = errors, message = "Въведените данни са невалидни. Моля, проверете всички полета." });
            }

            _logger.LogInformation("Received contract generation request for {ContractType}.", request.ContractType);

            var result = await _contractGeneratorService.GenerateContractAsync(request);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Contract successfully generated for {ContractType}. File: {FileName}", request.ContractType, result.GeneratedFileName);
                return File(result.GeneratedFileContent, result.ContentType, result.GeneratedFileName);
            }
            else
            {
                _logger.LogError("Failed to generate contract for {ContractType}. Error: {ErrorMessage}", request.ContractType, result.ErrorMessage);
                return BadRequest(result.ErrorMessage ?? "Неизвестна грешка при генериране на договор.");
            }
        }
    }
}
