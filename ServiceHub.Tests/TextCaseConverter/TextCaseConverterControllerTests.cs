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

namespace ServiceHub.Tests.TextCaseConverter
{
    public class TextCaseConverterControllerTests
    {
        private readonly Mock<ITextCaseConverterService> _mockTextCaseConverterService;
        private readonly Mock<ILogger<TextCaseConverterController>> _mockLogger;
        private readonly TextCaseConverterController _controller;

        public TextCaseConverterControllerTests()
        {
            _mockTextCaseConverterService = new Mock<ITextCaseConverterService>();
            _mockLogger = new Mock<ILogger<TextCaseConverterController>>();
            _controller = new TextCaseConverterController(_mockLogger.Object, _mockTextCaseConverterService.Object);
        }

        [Fact]
        public async Task ConvertCase_ReturnsOkResult_OnSuccess()
        {
      
            var request = new TextCaseConvertRequestModel { Text = "test", CaseType = "uppercase" };
            var expectedResponse = new TextCaseConvertResponseModel
            {
                ConvertedText = "TEST",
                Message = "Текстът е конвертиран в главни букви.",
                IsSuccess = true
            };

            _mockTextCaseConverterService
                .Setup(s => s.ConvertCaseAsync(It.IsAny<TextCaseConvertRequestModel>()))
                .ReturnsAsync(expectedResponse);

            
            var result = await _controller.ConvertCase(request);

           
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<TextCaseConvertResponseModel>(okResult.Value);

            Assert.Equal(expectedResponse.ConvertedText, actualResponse.ConvertedText);
            Assert.Equal(expectedResponse.Message, actualResponse.Message);
            Assert.True(actualResponse.IsSuccess);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid model state for TextCaseConvertRequestModel:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never 
            );
        }

        [Fact]
        public async Task ConvertCase_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            
            _controller.ModelState.AddModelError("Text", "The Text field is required.");
            var request = new TextCaseConvertRequestModel { Text = null, CaseType = "uppercase" };

           
            var result = await _controller.ConvertCase(request);

            
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey("Text"));
            Assert.Contains("The Text field is required.", (string[])modelState["Text"]);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid model state for TextCaseConvertRequestModel:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ConvertCase_ReturnsBadRequest_WhenServiceReturnsFailure()
        {
            
            var request = new TextCaseConvertRequestModel { Text = "test", CaseType = "invalid" };
            var serviceErrorMessage = "Невалиден тип конверсия. Поддържат се 'uppercase', 'lowercase', 'titlecase'.";
            var expectedResponse = new TextCaseConvertResponseModel
            {
                ConvertedText = "",
                Message = serviceErrorMessage,
                IsSuccess = false
            };

            _mockTextCaseConverterService
                .Setup(s => s.ConvertCaseAsync(It.IsAny<TextCaseConvertRequestModel>()))
                .ReturnsAsync(expectedResponse);

           
            var result = await _controller.ConvertCase(request);

          
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequestResult.Value));

            Assert.NotNull(errorObject);
            Assert.True(errorObject.ContainsKey("message"));
            Assert.Equal(serviceErrorMessage, errorObject["message"]);
        }
    }
}
