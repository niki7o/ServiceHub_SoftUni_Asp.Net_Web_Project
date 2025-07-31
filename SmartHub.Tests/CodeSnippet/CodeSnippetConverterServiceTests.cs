using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.CodeSnippet
{
    public class CodeSnippetConverterServiceTests
    {
        private readonly Mock<ILogger<CodeSnippetConverterService>> _mockLogger;
        private readonly CodeSnippetConverterService _service;

        public CodeSnippetConverterServiceTests()
        {
            _mockLogger = new Mock<ILogger<CodeSnippetConverterService>>();
            _service = new CodeSnippetConverterService(_mockLogger.Object);
        }

        [Fact]
        public async Task ConvertCodeAsync_ShouldReturnEmptyCodeAndMessage_WhenSourceCodeIsEmpty()
        {
            var request = new CodeSnippetConvertRequestModel
            {
                SourceCode = "",
                SourceLanguage = "c#",
                TargetLanguage = "python"
            };

            var response = await _service.ConvertCodeAsync(request);

            Assert.Equal("", response.ConvertedCode);
            Assert.Equal("Моля, въведете изходен код.", response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Source code is empty.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ConvertCodeAsync_ShouldReturnOriginalCodeAndWarning_WhenSourceAndTargetLanguagesAreSame()
        {
            var request = new CodeSnippetConvertRequestModel
            {
                SourceCode = "Console.WriteLine(\"Hello\");",
                SourceLanguage = "c#",
                TargetLanguage = "c#"
            };

            var response = await _service.ConvertCodeAsync(request);

            Assert.Equal(request.SourceCode, response.ConvertedCode);
            Assert.Equal("Изходният и целевият език не могат да бъдат еднакви. Върнат е оригиналният код.", response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Source and target languages are the same. Returning original code.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

       

        [Fact]
        public async Task ConvertCodeAsync_ShouldReturnUnsupportedMessage_ForUnknownTargetLanguage()
        {
            var request = new CodeSnippetConvertRequestModel
            {
                SourceCode = "Some code",
                SourceLanguage = "c#",
                TargetLanguage = "unknown"
            };

            var response = await _service.ConvertCodeAsync(request);

            Assert.Contains("// Неподдържана комбинация за конвертиране или опростена логика.", response.ConvertedCode);
            Assert.Contains("Избраната комбинация от езици не се поддържа от текущата опростена логика за конвертиране.", response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Attempting to convert code from {request.SourceLanguage} to {request.TargetLanguage}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Code conversion completed for {request.SourceLanguage} to {request.TargetLanguage}. Message:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        
        [Theory]
        [InlineData("myMethod", "MyMethod")]
        [InlineData("someVariable", "SomeVariable")]
        [InlineData("anotherOne", "AnotherOne")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void ConvertToPascalCase_ShouldConvertCorrectly(string input, string expected)
        {
            var result = _service.GetType().GetMethod("ConvertToPascalCase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                 .Invoke(_service, new object[] { input });
            Assert.Equal(expected, result);
        }

       
    }
}

