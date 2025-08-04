using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ServiceHub.Controllers;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.CvGenerator
{
    public class CvGeneratorControllerTests
    {
        private readonly Mock<ICvGeneratorService> _mockCvGeneratorService;
        private readonly Mock<ILogger<CvGeneratorController>> _mockLogger;
        private readonly CvGeneratorController _controller;

        public CvGeneratorControllerTests()
        {
            _mockCvGeneratorService = new Mock<ICvGeneratorService>();
            _mockLogger = new Mock<ILogger<CvGeneratorController>>();
            _controller = new CvGeneratorController(_mockCvGeneratorService.Object, _mockLogger.Object);
        }

        [Fact]
        public void CvGeneratorForm_ReturnsViewResult_WithCorrectViewPath()
        {
            var result = _controller.CvGeneratorForm();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_CvGeneratorForm.cshtml", viewResult.ViewName);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Serving CvGeneratorForm view.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateCv_ReturnsFileResult_OnSuccess()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Тест Име",
                Email = "test@example.com",
                Phone = "1234567890",
                Summary = "Кратко резюме.",
                Experience = "Опит.",
                Education = "Образование.",
                Skills = "Умения."
            };
            var generatedPdfContent = new byte[] { 0x01, 0x02, 0x03 };
            var generatedFileName = "Тест_Име_CV.pdf";
            var contentType = "application/pdf";

            _mockCvGeneratorService
                .Setup(s => s.GenerateCvAsync(It.IsAny<CvGenerateRequestModel>()))
                .ReturnsAsync(new CvGenerateResult
                {
                    IsSuccess = true,
                    GeneratedFileContent = generatedPdfContent,
                    GeneratedFileName = generatedFileName,
                    ContentType = contentType
                });

            var result = await _controller.GenerateCv(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(generatedPdfContent, fileResult.FileContents);
            Assert.Equal(generatedFileName, fileResult.FileDownloadName);
            Assert.Equal(contentType, fileResult.ContentType);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming CvGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Received CV generation request for {request.Name}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"CV successfully generated for {request.Name}. File: {generatedFileName}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateCv_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Name", "Name is required.");

            var request = new CvGenerateRequestModel();

            var result = await _controller.GenerateCv(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
          
            var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(badRequestResult.Value));

            Assert.NotNull(errorResponse);
            Assert.True(errorResponse.ContainsKey("errors"));
            Assert.True(errorResponse.ContainsKey("message"));

           
            var errors = JsonConvert.DeserializeObject<List<string>>(errorResponse["errors"].ToString());
            Assert.Contains("Name is required.", errors);

            var message = errorResponse["message"].ToString();
            Assert.Equal("Въведените данни са невалидни. Моля, проверете всички полета.", message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming CvGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Model validation failed for CvGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateCv_ReturnsBadRequest_WhenServiceFails()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Тест Име",
                Email = "test@example.com"
            };
            var errorMessage = "Service failed to generate CV.";

            _mockCvGeneratorService
                .Setup(s => s.GenerateCvAsync(It.IsAny<CvGenerateRequestModel>()))
                .ReturnsAsync(new CvGenerateResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage
                });

            var result = await _controller.GenerateCv(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming CvGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Received CV generation request for {request.Name}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Failed to generate CV for {request.Name}. Error: {errorMessage}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }
    
}
}
