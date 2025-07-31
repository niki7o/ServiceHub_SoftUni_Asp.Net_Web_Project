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

namespace ServiceHub.Tests.Contract
{

    public class ContractGeneratorControllerTests
    {
        private readonly Mock<IContractGeneratorService> _mockContractGeneratorService;
        private readonly Mock<ILogger<ContractGeneratorController>> _mockLogger;
        private readonly ContractGeneratorController _controller;

        public ContractGeneratorControllerTests()
        {
            _mockContractGeneratorService = new Mock<IContractGeneratorService>();
            _mockLogger = new Mock<ILogger<ContractGeneratorController>>();
            _controller = new ContractGeneratorController(_mockContractGeneratorService.Object, _mockLogger.Object);
        }

        [Fact]
        public void ContractGeneratorForm_ReturnsViewResult_WithCorrectViewPath()
        {
            var result = _controller.ContractGeneratorForm();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_ContractGeneratorForm.cshtml", viewResult.ViewName);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Serving ContractGeneratorForm view.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateContract_ReturnsFileResult_OnSuccess()
        {
            var request = new ContractGenerateRequestModel
            {
                ContractType = "Тестов договор",
                PartyA = "Страна А",
                PartyB = "Страна Б",
                ContractDate = DateTime.Now,
                ContractTerms = "Условия.",
                AdditionalInfo = ""
            };
            var generatedPdfContent = new byte[] { 0x01, 0x02, 0x03 };
            var generatedFileName = $"Тестов_договор_Страна_А_Страна_Б_{DateTime.Now:yyyyMMdd}.pdf";
            var contentType = "application/pdf";

            _mockContractGeneratorService
                .Setup(s => s.GenerateContractAsync(It.IsAny<ContractGenerateRequestModel>()))
                .ReturnsAsync(new ContractGenerateResult
                {
                    IsSuccess = true,
                    GeneratedFileContent = generatedPdfContent,
                    GeneratedFileName = generatedFileName,
                    ContentType = contentType
                });

            var result = await _controller.GenerateContract(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(generatedPdfContent, fileResult.FileContents);
            Assert.Contains($"{request.ContractType?.Replace(" ", "_")}_{request.PartyA?.Replace(" ", "_")}_{request.PartyB?.Replace(" ", "_")}", fileResult.FileDownloadName);
            Assert.Equal(contentType, fileResult.ContentType);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming ContractGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Received contract generation request for {request.ContractType}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Contract successfully generated for {request.ContractType}. File:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        
        [Fact]
        public async Task GenerateContract_ReturnsBadRequest_WhenServiceFails()
        {
            var request = new ContractGenerateRequestModel
            {
                ContractType = "Тестов договор",
                PartyA = "Страна А",
                PartyB = "Страна Б",
                ContractDate = DateTime.Now,
                ContractTerms = "Условия.",
                AdditionalInfo = ""
            };
            var errorMessage = "Service failed to generate contract.";

            _mockContractGeneratorService
                .Setup(s => s.GenerateContractAsync(It.IsAny<ContractGenerateRequestModel>()))
                .ReturnsAsync(new ContractGenerateResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage
                });

            var result = await _controller.GenerateContract(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming ContractGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Received contract generation request for {request.ContractType}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Failed to generate contract for {request.ContractType}. Error: {errorMessage}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }
    }
}


