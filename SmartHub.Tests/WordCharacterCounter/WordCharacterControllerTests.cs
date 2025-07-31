using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Controllers;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServiceHub.Tests.WordCharacterCounter
{
    public class WordCharacterControllerTests
    {
        private readonly Mock<IWordCharacterCounterService> _mockWordCharacterCounterService;
        private readonly Mock<ILogger<WordCharacterController>> _mockLogger;
        private readonly WordCharacterController _controller;

        public WordCharacterControllerTests()
        {
            _mockWordCharacterCounterService = new Mock<IWordCharacterCounterService>();
            _mockLogger = new Mock<ILogger<WordCharacterController>>();
            _controller = new WordCharacterController(_mockLogger.Object, _mockWordCharacterCounterService.Object);
        }

        [Fact]
        public async Task CountText_ReturnsOkResult_OnSuccess()
        {
         
            var request = new WordCharacterCountRequestModel { Text = "Hello world" };
            var expectedResponse = new WordCharacterCountResponseModel
            {
                WordCount = 2,
                CharCount = 11,
                LineCount = 1,
                Message = "Преброяването е успешно."
            };

            _mockWordCharacterCounterService
                .Setup(s => s.CountTextAsync(It.IsAny<WordCharacterCountRequestModel>()))
                .ReturnsAsync(expectedResponse);

            
            var result = await _controller.CountText(request);

          
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<WordCharacterCountResponseModel>(okResult.Value);

            Assert.Equal(expectedResponse.WordCount, actualResponse.WordCount);
            Assert.Equal(expectedResponse.CharCount, actualResponse.CharCount);
            Assert.Equal(expectedResponse.LineCount, actualResponse.LineCount);
            Assert.Equal(expectedResponse.Message, actualResponse.Message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid model state for WordCharacterCountRequestModel:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never 
            );
        }

        [Fact]
        public async Task CountText_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
          
            _controller.ModelState.AddModelError("Text", "The Text field is required.");
            var request = new WordCharacterCountRequestModel { Text = null };

       
            var result = await _controller.CountText(request);

           
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey("Text"));
            Assert.Contains("The Text field is required.", (string[])modelState["Text"]);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid model state for WordCharacterCountRequestModel:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CountText_ReturnsOkResult_ForEmptyTextFromService()
        {
        
            var request = new WordCharacterCountRequestModel { Text = "" };
            var expectedResponse = new WordCharacterCountResponseModel
            {
                WordCount = 0,
                CharCount = 0,
                LineCount = 0,
                Message = "Въведеният текст е празен."
            };

            _mockWordCharacterCounterService
                .Setup(s => s.CountTextAsync(It.IsAny<WordCharacterCountRequestModel>()))
                .ReturnsAsync(expectedResponse);

            
            var result = await _controller.CountText(request);

           
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<WordCharacterCountResponseModel>(okResult.Value);

            Assert.Equal(expectedResponse.WordCount, actualResponse.WordCount);
            Assert.Equal(expectedResponse.CharCount, actualResponse.CharCount);
            Assert.Equal(expectedResponse.LineCount, actualResponse.LineCount);
            Assert.Equal(expectedResponse.Message, actualResponse.Message);
        }
    }
}
