using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    
        [Authorize]
        public class InvoiceGeneratorController : Controller
        {
            private readonly IInvoiceGeneratorService _invoiceGeneratorService;
            private readonly ILogger<InvoiceGeneratorController> _logger;

            public InvoiceGeneratorController(IInvoiceGeneratorService invoiceGeneratorService, ILogger<InvoiceGeneratorController> logger)
            {
                _invoiceGeneratorService = invoiceGeneratorService;
                _logger = logger;
            }

           
            [HttpGet("/InvoiceGenerator/InvoiceGeneratorForm")]
            public IActionResult InvoiceGeneratorForm()
            {
                _logger.LogInformation("Serving InvoiceGeneratorForm view.");
              
                ViewData["ServiceId"] = "B422F89B-E7A3-4130-B899-7B56010007E0";
                return View("~/Views/Service/_InvoiceGeneratorForm.cshtml");
            }

            
            [HttpPost("/api/InvoiceGenerator/generate")]
            public async Task<IActionResult> GenerateInvoice([FromBody] InvoiceGenerateRequestModel request)
            {
                _logger.LogDebug("Incoming InvoiceGenerateRequestModel: {@Request}", request);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                    .Select(e => e.ErrorMessage)
                                                    .ToList();
                    _logger.LogWarning("Model validation failed for InvoiceGenerateRequestModel: {Errors}", string.Join("; ", errors));
                    return BadRequest(new { errors = errors, message = "Въведените данни са невалидни. Моля, проверете всички полета." });
                }

                _logger.LogInformation("Received invoice generation request for invoice number {InvoiceNumber}.", request.InvoiceNumber);

                var result = await _invoiceGeneratorService.GenerateInvoiceAsync(request);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Invoice {InvoiceNumber} successfully generated. File: {FileName}", request.InvoiceNumber, result.GeneratedFileName);
                    return File(result.GeneratedFileContent, result.ContentType, result.GeneratedFileName);
                }
                else
                {
                    _logger.LogError("Failed to generate invoice {InvoiceNumber}. Error: {ErrorMessage}", request.InvoiceNumber, result.ErrorMessage);
                    return BadRequest(result.ErrorMessage ?? "Неизвестна грешка при генериране на фактура.");
                }
            }
        }
    
}
