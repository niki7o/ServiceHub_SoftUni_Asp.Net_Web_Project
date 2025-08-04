using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceHub.Common;
using ServiceHub.Core.Models;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;

namespace ServiceHub.Controllers
{
    [Route("FileConverter")]
    public class FileConverterController : Controller
    {
        private readonly ILogger<FileConverterController> _logger;
        private readonly IServiceDispatcher _serviceDispatcher;
        private readonly UserManager<ApplicationUser> _userManager;

        public FileConverterController(
            ILogger<FileConverterController> logger,
            IServiceDispatcher serviceDispatcher,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _serviceDispatcher = serviceDispatcher;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.ServiceId = ServiceConstants.FileConverterServiceId;
                _logger.LogInformation($"Loading File Converter Index view with ServiceId: {ServiceConstants.FileConverterServiceId}");

                var user = await _userManager.GetUserAsync(User);
                bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
                bool isBusinessUser = user != null && await _userManager.IsInRoleAsync(user, "BusinessUser");
                bool isPremiumUserCalculated = isAdmin || isBusinessUser;
                ViewBag.IsBusinessUserOrAdmin = isPremiumUserCalculated;

                _logger.LogInformation($"FileConverterController.Index: User roles - IsAdmin: {isAdmin}, IsBusinessUser: {isBusinessUser}. Calculated IsBusinessUserOrAdmin for ViewBag: {isPremiumUserCalculated}");

                ViewBag.SupportedFormats = new List<string> { "pdf", "docx", "txt", "jpg", "png", "xlsx", "csv" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing ServiceConstants.FileConverterServiceId. Ensure it is a valid GUID.");
               
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View("~/Views/Service/_FileConverterForm.cshtml");
        }

        [HttpPost("Convert")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Convert(
            [FromForm(Name = "ServiceId")] Guid serviceId,
            [FromForm(Name = "FileContent")] IFormFile fileContent,
            [FromForm(Name = "OriginalFileName")] string? originalFileNameInput,
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

            string finalOriginalFileName = string.IsNullOrWhiteSpace(originalFileNameInput)
                                            ? fileContent.FileName
                                            : originalFileNameInput;

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
            bool isBusinessUser = user != null && await _userManager.IsInRoleAsync(user, "BusinessUser");
            bool isPremiumUser = isAdmin || isBusinessUser;

            _logger.LogInformation($"FileConverterController.Convert: User roles - IsAdmin: {isAdmin}, IsBusinessUser: {isBusinessUser}. IsPremiumUser sent to service: {isPremiumUser}");


            var serviceRequest = new FileConvertRequest
            {
                ServiceId = serviceId,
                FileContent = fileBytes,
                OriginalFileName = finalOriginalFileName,
                TargetFormat = targetFormat,
                PerformOCRIfApplicable = performOCRIfApplicable,
                IsPremiumUser = isPremiumUser
            };

            _logger.LogInformation($"Dispatching request for file conversion: ServiceId={serviceRequest.ServiceId}, FileName={serviceRequest.OriginalFileName}, TargetFormat={serviceRequest.TargetFormat}, OCR={serviceRequest.PerformOCRIfApplicable}, IsPremiumUser={serviceRequest.IsPremiumUser}");

            BaseServiceResponse response = await _serviceDispatcher.DispatchAsync(serviceRequest);

            if (response.IsSuccess)
            {
                if (response is FileConvertResult fileConvertResult)
                {
                    _logger.LogInformation($"File conversion successful for '{fileConvertResult.ConvertedFileName}'.");
                    if (fileConvertResult.ConvertedFileContent != null && fileConvertResult.ConvertedFileContent.Length > 0)
                    {
                        string finalContentType = fileConvertResult.ContentType ?? "application/octet-stream";
                        if (fileConvertResult.TargetFormat.ToLowerInvariant() == "pdf")
                        {
                            finalContentType = "application/pdf";
                        }

                        return File(fileConvertResult.ConvertedFileContent, finalContentType, fileConvertResult.ConvertedFileName);
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
