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

namespace ServiceHub.Tests.WordCharacterCounter
{
    public class WordCharacterCounterServiceTests
    {
        private readonly Mock<ILogger<WordCharacterCounterService>> _mockLogger;
        private readonly WordCharacterCounterService _service;

        public WordCharacterCounterServiceTests()
        {
            _mockLogger = new Mock<ILogger<WordCharacterCounterService>>();
            _service = new WordCharacterCounterService(_mockLogger.Object);
        }

        [Theory]
        [InlineData("Hello world", 2, 11, 1, "Преброяването е успешно.")]
        [InlineData("One two three four five", 5, 23, 1, "Преброяването е успешно.")]
        [InlineData("Line 1\nLine 2\r\nLine 3", 6, 21, 3, "Преброяването е успешно.")]
        [InlineData("SingleWord", 1, 10, 1, "Преброяването е успешно.")]
       
        [InlineData("  leading and trailing spaces  ", 4, 31, 1, "Преброяването е успешно.")]
        [InlineData("Multiple   spaces   between   words", 4, 35, 1, "Преброяването е успешно.")]
        [InlineData("Punctuation. Marks! Test?", 3, 25, 1, "Преброяването е успешно.")]
        [InlineData("123 456 789", 3, 11, 1, "Преброяването е успешно.")]
        [InlineData("Line1\nLine2", 2, 11, 2, "Преброяването е успешно.")]
        [InlineData("Line1\rLine2", 2, 11, 2, "Преброяването е успешно.")]
        [InlineData("Line1\r\nLine2", 2, 12, 2, "Преброяването е успешно.")]
        public async Task CountTextAsync_ShouldReturnCorrectCounts(
            string inputText, int expectedWordCount, int expectedCharCount, int expectedLineCount, string expectedMessage)
        {
            
            var request = new WordCharacterCountRequestModel { Text = inputText };

            
            var response = await _service.CountTextAsync(request);

           
            Assert.NotNull(response);
            Assert.Equal(expectedWordCount, response.WordCount);
            Assert.Equal(expectedCharCount, response.CharCount);
            Assert.Equal(expectedLineCount, response.LineCount);
            Assert.Equal(expectedMessage, response.Message);

         
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"CountTextAsync received request.Text: '{inputText}'")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Text counted: Words={expectedWordCount}, Chars={expectedCharCount}, Lines={expectedLineCount}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
           
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never
            );
        }

        [Theory]
        [InlineData("", 0, 0, 0, "Въведеният текст е празен.")]
        [InlineData("   ", 0, 0, 0, "Въведеният текст е празен.")]
        [InlineData(null, 0, 0, 0, "Въведеният текст е празен.")] 
        public async Task CountTextAsync_ShouldReturnZeroCountsAndWarningForEmptyOrNullText(
            string inputText, int expectedWordCount, int expectedCharCount, int expectedLineCount, string expectedMessage)
        {
          
            var request = new WordCharacterCountRequestModel { Text = inputText };

            
            var response = await _service.CountTextAsync(request);

           
            Assert.NotNull(response);
            Assert.Equal(expectedWordCount, response.WordCount);
            Assert.Equal(expectedCharCount, response.CharCount);
            Assert.Equal(expectedLineCount, response.LineCount);
            Assert.Equal(expectedMessage, response.Message);

       
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Attempted to count empty or whitespace text.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
       
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"CountTextAsync received request.Text: '{inputText ?? "[NULL]"}'")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
         
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Text counted:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never
            );
        }
    }
}
