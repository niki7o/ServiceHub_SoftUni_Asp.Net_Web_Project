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

namespace ServiceHub.Tests.RandomPasswordGenerator
{
    public class RandomPasswordGeneratorControllerTests
    {
        private readonly Mock<IRandomPasswordGeneratorService> _mockPasswordGeneratorService;
        private readonly Mock<ILogger<RandomPasswordGeneratorController>> _mockLogger;
        private readonly RandomPasswordGeneratorController _controller;

        public RandomPasswordGeneratorControllerTests()
        {
            _mockPasswordGeneratorService = new Mock<IRandomPasswordGeneratorService>();
            _mockLogger = new Mock<ILogger<RandomPasswordGeneratorController>>();
            _controller = new RandomPasswordGeneratorController(_mockLogger.Object, _mockPasswordGeneratorService.Object);
        }

        [Fact]
        public async Task GeneratePassword_ReturnsOkResult_OnSuccess()
        {
            var request = new PasswordGenerateRequestModel
            {
                Length = 10,
                IncludeLowercase = true,
                IncludeUppercase = true,
                IncludeDigits = true,
                IncludeSpecialChars = true
            };
            var expectedResponse = new PasswordGenerateResponseModel
            {
                GeneratedPassword = "TestPassword1!",
                Message = "Паролата е генерирана успешно."
            };

            _mockPasswordGeneratorService
                .Setup(s => s.GeneratePasswordAsync(It.IsAny<PasswordGenerateRequestModel>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.GeneratePassword(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<PasswordGenerateResponseModel>(okResult.Value);

            Assert.Equal(expectedResponse.GeneratedPassword, actualResponse.GeneratedPassword);
            Assert.Equal(expectedResponse.Message, actualResponse.Message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning, // Log level for invalid model state
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid model state for PasswordGenerateRequestModel:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never // Should not log invalid model state if valid
            );
        }

        [Fact]
        public async Task GeneratePassword_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Length", "The Length field is required.");

            var request = new PasswordGenerateRequestModel();

            var result = await _controller.GeneratePassword(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey("Length"));
            Assert.Contains("The Length field is required.", (string[])modelState["Length"]);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid model state for PasswordGenerateRequestModel:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GeneratePassword_ReturnsBadRequest_WhenServiceReturnsErrorMessage()
        {
            var request = new PasswordGenerateRequestModel
            {
                Length = 0, // Invalid length to trigger service error
                IncludeLowercase = true
            };
            var serviceErrorMessage = "Дължината на паролата трябва да е между 1 и 128 символа.";
            var expectedResponse = new PasswordGenerateResponseModel
            {
                GeneratedPassword = "",
                Message = serviceErrorMessage
            };

            _mockPasswordGeneratorService
                .Setup(s => s.GeneratePasswordAsync(It.IsAny<PasswordGenerateRequestModel>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.GeneratePassword(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(badRequestResult.Value));

            Assert.NotNull(errorObject);
            Assert.True(errorObject.ContainsKey("message"));
            Assert.Equal(serviceErrorMessage, errorObject["message"]);
        }
    }
}
