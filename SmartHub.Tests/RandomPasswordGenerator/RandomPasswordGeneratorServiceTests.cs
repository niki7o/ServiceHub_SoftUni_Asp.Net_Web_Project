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

namespace ServiceHub.Tests.RandomPasswordGenerator
{
    public class RandomPasswordGeneratorServiceTests
    {
        private readonly Mock<ILogger<RandomPasswordGeneratorService>> _mockLogger;
        private readonly RandomPasswordGeneratorService _service;

        private const string lower = "abcdefghijklmnopqrstuvwxyz";
        private const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string digits = "0123456789";
        private const string special = "!@#$%^&*()_+{}[]:;<>,.?/~";

        public RandomPasswordGeneratorServiceTests()
        {
            _mockLogger = new Mock<ILogger<RandomPasswordGeneratorService>>();
            _service = new RandomPasswordGeneratorService(_mockLogger.Object);
        }

        

        [Fact]
        public async Task GeneratePasswordAsync_ShouldReturnErrorMessageForLengthLessThanOne()
        {
            var request = new PasswordGenerateRequestModel { Length = 0 };
            var response = await _service.GeneratePasswordAsync(request);

            Assert.NotNull(response);
            Assert.Equal("", response.GeneratedPassword);
            Assert.Equal("Дължината на паролата трябва да е между 1 и 128 символа.", response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid password length requested: 0")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GeneratePasswordAsync_ShouldReturnErrorMessageForLengthGreaterThan128()
        {
            var request = new PasswordGenerateRequestModel { Length = 129 };
            var response = await _service.GeneratePasswordAsync(request);

            Assert.NotNull(response);
            Assert.Equal("", response.GeneratedPassword);
            Assert.Equal("Дължината на паролата трябва да е между 1 и 128 символа.", response.Message);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid password length requested: 129")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        
    }
}
