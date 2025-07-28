using Microsoft.AspNetCore.Mvc;
using ServiceHub.Common;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [Route("FileConverter")]
    public class FileConverterController : Controller
    {
        private readonly ILogger<FileConverterController> _logger;
        private readonly IServiceDispatcher _serviceDispatcher;

        public FileConverterController(ILogger<FileConverterController> logger, IServiceDispatcher serviceDispatcher)
        {
            _logger = logger;
            _serviceDispatcher = serviceDispatcher;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            try
            {
                ViewBag.ServiceId = ServiceConstants.FileConverterServiceId;
                _logger.LogInformation($"Loading File Converter Index view with ServiceId: {ServiceConstants.FileConverterServiceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing ServiceConstants.FileConverterServiceId. Ensure it is a valid GUID.");
                return View("Error", new { RequestId = "Invalid ServiceId Configuration" });
            }

            return View("~/Views/Service/_FileConverterForm.cshtml");
        }

        [HttpPost("Convert")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Convert(
            [FromForm(Name = "ServiceId")] Guid serviceId,
            [FromForm(Name = "FileContent")] IFormFile fileContent,
            [FromForm(Name = "OriginalFileName")] string? originalFileName,
            [FromForm(Name = "TargetFormat")] string targetFormat,
            [FromForm(Name = "PerformOCRIfApplicable")] bool performOCRIfApplicable)
        {
            if (serviceId == Guid.Empty)
            {
                _logger.LogWarning("Липсва ServiceId в заявката.");
                return BadRequest(new { message = "Идентификаторът на услугата е задължителен." });
            }

            if (serviceId != ServiceConstants.FileConverterServiceId)
            {
                _logger.LogWarning($"Невалиден ServiceId: {serviceId}. Очакван: {ServiceConstants.FileConverterServiceId}");
                return BadRequest(new { message = "Невалиден идентификатор на услугата." });
            }

            if (fileContent == null || fileContent.Length == 0)
            {
                _logger.LogWarning("Липсва съдържание на файл.");
                return BadRequest(new { message = "Моля, изберете файл за конвертиране." });
            }

            if (string.IsNullOrWhiteSpace(targetFormat))
            {
                _logger.LogWarning("Липсва целеви формат.");
                return BadRequest(new { message = "Моля, изберете целеви формат." });
            }

            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await fileContent.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var serviceRequest = new FileConvertRequest
            {
                ServiceId = serviceId,
                FileContent = fileBytes,
                OriginalFileName = originalFileName ?? fileContent.FileName,
                TargetFormat = targetFormat,
                PerformOCRIfApplicable = performOCRIfApplicable
            };

            _logger.LogInformation($"Dispatching request for file conversion: ServiceId={serviceRequest.ServiceId}, FileName={serviceRequest.OriginalFileName}, TargetFormat={serviceRequest.TargetFormat}, OCR={serviceRequest.PerformOCRIfApplicable}");

            BaseServiceResponse response = await _serviceDispatcher.DispatchAsync(serviceRequest);

            if (response.IsSuccess)
            {
                if (response is FileConvertResult fileConvertResult)
                {
                    _logger.LogInformation($"File conversion successful for '{fileConvertResult.ConvertedFileName}'.");
                    if (fileConvertResult.ConvertedFileContent != null && fileConvertResult.ConvertedFileContent.Length > 0)
                    {
                        return File(fileConvertResult.ConvertedFileContent, fileConvertResult.ContentType ?? "application/octet-stream", fileConvertResult.ConvertedFileName);
                    }
                    else
                    {
                        _logger.LogWarning("Conversion was successful, but no file content was returned.");
                        return StatusCode(500, new { message = "Файлът е успешно конвертиран, но не бе получено съдържание за изтегляне." });
                    }
                }
                else
                {
                    _logger.LogError("ServiceDispatcher returned success, but response was not FileConvertResult.");
                    return StatusCode(500, new { message = "Вътрешна грешка: Неочакван отговор от услугата." });
                }
            }
            else
            {
                _logger.LogError($"Service execution failed for ID: {serviceRequest.ServiceId}. Error: {response.ErrorMessage}");
                return BadRequest(new { message = $"Грешка при конвертиране на файла: {response.ErrorMessage ?? "Неизвестна грешка."}" });
            }
        }
    
    }
}
