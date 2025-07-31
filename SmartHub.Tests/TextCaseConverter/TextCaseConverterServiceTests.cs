using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.TextCaseConverter
{
    public class TextCaseConverterServiceTests
    {
        private readonly Mock<ILogger<TextCaseConverterService>> _mockLogger;
        private readonly TextCaseConverterService _service;

        public TextCaseConverterServiceTests()
        {
            _mockLogger = new Mock<ILogger<TextCaseConverterService>>();
            _service = new TextCaseConverterService(_mockLogger.Object);
        }

        [Theory]
        [InlineData("hello world", "uppercase", "HELLO WORLD", "Текстът е конвертиран в главни букви.")]
        [InlineData("HELLO WORLD", "lowercase", "hello world", "Текстът е конвертиран в малки букви.")]
        [InlineData("hello world", "titlecase", "Hello World", "Текстът е конвертиран в заглавен регистър.")]
        [InlineData("another test string", "uppercase", "ANOTHER TEST STRING", "Текстът е конвертиран в главни букви.")]
        [InlineData("ANOTHER TEST STRING", "lowercase", "another test string", "Текстът е конвертиран в малки букви.")]
        [InlineData("another test string", "titlecase", "Another Test String", "Текстът е конвертиран в заглавен регистър.")]
        [InlineData("some-kebab-case", "titlecase", "Some-Kebab-Case", "Текстът е конвертиран в заглавен регистър.")]
        public async Task ConvertCaseAsync_ShouldConvertTextCorrectly(
            string inputText, string caseType, string expectedOutput, string expectedMessage)
        {
         
            var request = new TextCaseConvertRequestModel { Text = inputText, CaseType = caseType };

         
            var response = await _service.ConvertCaseAsync(request);

            
            Assert.NotNull(response);
            Assert.True(response.IsSuccess);
            Assert.Equal(expectedOutput, response.ConvertedText);
            Assert.Equal(expectedMessage, response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Text converted to {caseType}: Original='{inputText}', Converted='{expectedOutput}'")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ConvertCaseAsync_ShouldReturnErrorMessage_WhenTextIsEmpty()
        {
            
            var request = new TextCaseConvertRequestModel { Text = "", CaseType = "uppercase" };

            
            var response = await _service.ConvertCaseAsync(request);

         
            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.Equal("", response.ConvertedText);
            Assert.Equal("Моля, въведете текст.", response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never 
            );
        }

        [Fact]
        public async Task ConvertCaseAsync_ShouldReturnErrorMessage_WhenCaseTypeIsInvalid()
        {
         
            var request = new TextCaseConvertRequestModel { Text = "some text", CaseType = "invalidcase" };
            var expectedMessage = "Невалиден тип конверсия. Поддържат се 'uppercase', 'lowercase', 'titlecase'.";

           
            var response = await _service.ConvertCaseAsync(request);

           
            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.Equal("", response.ConvertedText);
            Assert.Equal(expectedMessage, response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Invalid case type provided: {request.CaseType}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }
    }
}
