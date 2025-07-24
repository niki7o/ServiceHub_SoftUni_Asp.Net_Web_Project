using Microsoft.AspNetCore.Mvc;
using ServiceHub.Common;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class FileConverterController : ControllerBase
    {
        private readonly ILogger<FileConverterController> _logger;
        private readonly IServiceDispatcher _serviceDispatcher;

        public FileConverterController(ILogger<FileConverterController> logger, IServiceDispatcher serviceDispatcher)
        {
            _logger = logger;
            _serviceDispatcher = serviceDispatcher;
        }

        [HttpPost("convert")] // Full route: /api/FileConverter/convert
        [Consumes("multipart/form-data")] // Important for file uploads
        public async Task<IActionResult> ConvertFile([FromForm] FileConvertRequest request)
        {
            // The FileConvertRequest model should handle basic validation (e.g., [Required])
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for FileConvertRequest: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            // Ensure the file is provided in the form data
            if (Request.Form.Files == null || !Request.Form.Files.Any())
            {
                _logger.LogWarning("No file uploaded for conversion.");
                return BadRequest(new { message = "Моля, качете файл за конвертиране." });
            }

            var file = Request.Form.Files[0]; // Get the first file

            if (file.Length == 0)
            {
                _logger.LogWarning("Uploaded file is empty.");
                return BadRequest(new { message = "Каченият файл е празен." });
            }

            // Read file content into a byte array
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            request.FileContent = memoryStream.ToArray();
            request.OriginalFileName = file.FileName; // Ensure original filename is set
            request.ServiceId = ServiceConstants.FileConverterServiceId; // Ensure correct service ID

            _logger.LogInformation("Received file '{FileName}' for conversion to '{TargetFormat}'. Size: {FileSize} bytes.",
                                   request.OriginalFileName, request.TargetFormat, request.FileContent.Length);

            // Dispatch the request to the appropriate service handler
            var response = await _serviceDispatcher.DispatchAsync(request);

            if (response.IsSuccess)
            {
                _logger.LogInformation("File conversion successful for '{FileName}'.", request.OriginalFileName);
                var fileConvertResult = response as FileConvertResult;
                if (fileConvertResult != null && fileConvertResult.ConvertedFileContent != null)
                {
                    // Return the converted file as a downloadable file
                    return File(fileConvertResult.ConvertedFileContent, fileConvertResult.ContentType ?? "application/octet-stream", fileConvertResult.ConvertedFileName ?? "converted_file");
                }
                else
                {
                    _logger.LogError("File conversion reported success but no converted content was returned for '{FileName}'.", request.OriginalFileName);
                    return StatusCode(500, new { message = "Конвертирането е успешно, но не беше получен конвертиран файл." });
                }
            }
            else
            {
                _logger.LogError("File conversion failed for '{FileName}'. Error: {ErrorMessage}", request.OriginalFileName, response.ErrorMessage);
                return BadRequest(new { message = response.ErrorMessage ?? "Възникна грешка при конвертиране на файла." });
            }
        }
    }
}
