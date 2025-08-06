using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Controllers;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Data.Models;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;
using Newtonsoft.Json.Linq; 

namespace ServiceHub.Tests.CodeSnippet
{
    public class CodeSnippetConverterControllerTests
    {
        private readonly Mock<ICodeSnippetConverterService> _mockCodeSnippetConverterService;
        private readonly Mock<ILogger<CodeSnippetConverterController>> _mockLogger;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly CodeSnippetConverterController _controller;

        public CodeSnippetConverterControllerTests()
        {
            _mockCodeSnippetConverterService = new Mock<ICodeSnippetConverterService>();

            _mockLogger = new Mock<ILogger<CodeSnippetConverterController>>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new CodeSnippetConverterController(
                _mockCodeSnippetConverterService.Object,
                _mockLogger.Object,
                _mockUserManager.Object);
        }

        private void SetupUserContext(ApplicationUser user, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, It.IsAny<string>()))
                            .ReturnsAsync((ApplicationUser u, string role) => roles.Contains(role));
        }


        [Fact]
        public async Task ConvertCode_ReturnsOkResult_WithConvertedCode_OnSuccess_User()
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

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            SetupUserContext(user, "User");

            _mockCodeSnippetConverterService
                .Setup(s => s.ConvertCodeAsync(request, false))
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

        [Fact]
        public async Task ConvertCode_ReturnsOkResult_WithConvertedCode_OnSuccess_BusinessUser()
        {
            var request = new CodeSnippetConvertRequestModel
            {
                SourceCode = "console.log('Hello');",
                SourceLanguage = "javascript",
                TargetLanguage = "c#"
            };
            var serviceResponse = new CodeSnippetConvertResponseModel
            {
                ConvertedCode = "Console.WriteLine(\"Hello\");",
                Message = "Conversion successful."
            };

            var user = new ApplicationUser { Id = "business-user-id", UserName = "businessuser" };
            SetupUserContext(user, "BusinessUser");

            _mockCodeSnippetConverterService
                .Setup(s => s.ConvertCodeAsync(request, true))
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

        [Fact]
        public async Task ConvertCode_ReturnsForbidResult_ForLockedLanguage_NonBusinessUser()
        {
            var request = new CodeSnippetConvertRequestModel
            {
                SourceCode = "console.log('test');",
                SourceLanguage = "javascript",
                TargetLanguage = "c#"
            };
            var serviceResponse = new CodeSnippetConvertResponseModel
            {
                ConvertedCode = "// Достъпът до JavaScript и PHP конвертиране е само за Бизнес Потребители.",
                Message = "Достъпът до JavaScript и PHP конвертиране е само за Бизнес Потребители. Моля, надстройте акаунта си."
            };

            var user = new ApplicationUser { Id = "regular-user-id", UserName = "regularuser" };
            SetupUserContext(user, "User");

            _mockCodeSnippetConverterService
                .Setup(s => s.ConvertCodeAsync(request, false))
                .ReturnsAsync(serviceResponse);

            var result = await _controller.ConvertCode(request);

            Assert.IsType<ForbidResult>(result);
      

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Received code conversion request in API controller.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockCodeSnippetConverterService.Verify(s => s.ConvertCodeAsync(request, false), Times.Once);
        }

       
        
    }
}
