using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using ServiceHub.Controllers;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.CodeSnippet
{
    public class CodeSnippetConverterControllerTests
    {
        private readonly Mock<ICodeSnippetConverterService> _mockCodeSnippetConverterService;
        private readonly Mock<ILogger<CodeSnippetConverterController>> _mockLogger;
        private readonly CodeSnippetConverterController _controller;

        public CodeSnippetConverterControllerTests()
        {
            _mockCodeSnippetConverterService = new Mock<ICodeSnippetConverterService>();
            _mockLogger = new Mock<ILogger<CodeSnippetConverterController>>();
            _controller = new CodeSnippetConverterController(_mockCodeSnippetConverterService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ConvertCode_ReturnsOkResult_WithConvertedCode_OnSuccess()
        {
            var request = new CodeSnippetConvertRequestModel
            {
                SourceCode = "Console.WriteLine(\"Hello\");",
                SourceLanguage = "c#",
                TargetLanguage = "python"
            };
            var serviceResponse = new CodeSnippetConvertResponseModel
            {
                ConvertedCode = "print(\"Hello\")",
                Message = "Conversion successful."
            };

            _mockCodeSnippetConverterService
                .Setup(s => s.ConvertCodeAsync(request))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.ConvertCode(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<CodeSnippetConvertResponseModel>(okResult.Value);
            Assert.Equal(serviceResponse.ConvertedCode, actualResponse.ConvertedCode);
            Assert.Equal(serviceResponse.Message, actualResponse.Message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Received code conversion request in API controller.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Code conversion successful via service.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        
    }
}
