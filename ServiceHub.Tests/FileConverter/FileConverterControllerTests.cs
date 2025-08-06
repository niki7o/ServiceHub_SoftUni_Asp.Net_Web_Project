using Xunit;
using Moq;
using ServiceHub.Controllers;
using ServiceHub.Services.Interfaces;
using ServiceHub.Core.Models.Service.FileConverter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceHub.Common;
using System.Collections.Generic;
using System.Threading;
using ServiceHub.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace ServiceHub.Tests.FileConverter
{
    public class MockableBaseServiceResponse : BaseServiceResponse
    {
        public override bool IsSuccess { get; set; }
        public override string ErrorMessage { get; set; } = string.Empty;
    }

    public class FileConverterControllerTests
    {
        private readonly Mock<ILogger<FileConverterController>> _mockLogger;
        private readonly Mock<IServiceDispatcher> _mockServiceDispatcher;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly FileConverterController _controller;

        public FileConverterControllerTests()
        {
            _mockLogger = new Mock<ILogger<FileConverterController>>();
            _mockServiceDispatcher = new Mock<IServiceDispatcher>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new FileConverterController(_mockLogger.Object, _mockServiceDispatcher.Object, _mockUserManager.Object);
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

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            var tempData = new TempDataDictionary(httpContext, new Mock<ITempDataProvider>().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "Admin"))
                            .ReturnsAsync(roles.Contains("Admin"));
            _mockUserManager.Setup(um => um.IsInRoleAsync(user, "BusinessUser"))
                            .ReturnsAsync(roles.Contains("BusinessUser"));
        }

        private void SetupEmptyUserContext()
        {
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, new Mock<ITempDataProvider>().Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempData;

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                            .ReturnsAsync(false);
        }


        [Fact]
        public async Task Index_ShouldReturnFileConverterFormView_NonPremiumUser()
        {
            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            SetupUserContext(user, "User");

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_FileConverterForm.cshtml", viewResult.ViewName);
            Assert.Equal(ServiceConstants.FileConverterServiceId, viewResult.ViewData["ServiceId"]);
            Assert.False((bool)viewResult.ViewData["IsBusinessUserOrAdmin"]);
            Assert.NotNull(viewResult.ViewData["SupportedFormats"]);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Loading File Converter Index view with ServiceId: {ServiceConstants.FileConverterServiceId}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("FileConverterController.Index: User roles - IsAdmin: False, IsBusinessUser: False. Calculated IsBusinessUserOrAdmin for ViewBag: False")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Index_ShouldReturnFileConverterFormView_PremiumUser()
        {
            var user = new ApplicationUser { Id = "business-user-id", UserName = "businessuser" };
            SetupUserContext(user, "BusinessUser");

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_FileConverterForm.cshtml", viewResult.ViewName);
            Assert.Equal(ServiceConstants.FileConverterServiceId, viewResult.ViewData["ServiceId"]);
            Assert.True((bool)viewResult.ViewData["IsBusinessUserOrAdmin"]);
            Assert.NotNull(viewResult.ViewData["SupportedFormats"]);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Loading File Converter Index view with ServiceId: {ServiceConstants.FileConverterServiceId}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("FileConverterController.Index: User roles - IsAdmin: False, IsBusinessUser: True. Calculated IsBusinessUserOrAdmin for ViewBag: True")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Convert_ShouldReturnBadRequest_WhenServiceIdIsInvalid()
        {
            SetupEmptyUserContext();

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("dummy")));
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.Convert(Guid.Empty, mockFile.Object, null, "pdf", false);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var jsonResult = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Идентификаторът на услугата е задължителен.".Trim(), jsonResult["message"].ToString().Trim());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Липсва ServiceId в заявката.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Convert_ShouldReturnBadRequest_WhenServiceIdDoesNotMatchConstant()
        {
            var wrongServiceId = Guid.NewGuid();
            SetupEmptyUserContext();

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("dummy")));
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.Convert(wrongServiceId, mockFile.Object, null, "pdf", false);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var jsonResult = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Невалиден идентификатор на услугата.".Trim(), jsonResult["message"].ToString().Trim());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Невалиден ServiceId: {wrongServiceId}. Очакван: {ServiceConstants.FileConverterServiceId}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Convert_ShouldReturnBadRequest_WhenFileContentIsMissing()
        {
            var serviceId = ServiceConstants.FileConverterServiceId;
            SetupEmptyUserContext();

            var result = await _controller.Convert(serviceId, null, null, "pdf", false);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var jsonResult = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Моля, изберете файл за конвертиране.".Trim(), jsonResult["message"].ToString().Trim());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Липсва съдържание на файл.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Convert_ShouldReturnBadRequest_WhenTargetFormatIsMissing()
        {
            var serviceId = ServiceConstants.FileConverterServiceId;
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            mockFile.Setup(f => f.Length).Returns(10);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("test")));
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            SetupEmptyUserContext();

            var result = await _controller.Convert(serviceId, mockFile.Object, null, "", false);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var jsonResult = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Моля, изберете целеви формат.".Trim(), jsonResult["message"].ToString().Trim());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Липсва целеви формат.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Convert_ShouldReturnBadRequest_WhenConversionFails()
        {
            var serviceId = ServiceConstants.FileConverterServiceId;
            var testFileContent = Encoding.UTF8.GetBytes("test content");

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("original.txt");
            mockFile.Setup(f => f.Length).Returns(testFileContent.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(testFileContent));
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Callback<Stream, CancellationToken>(async (stream, token) =>
                    {
                        using (var ms = new MemoryStream(testFileContent))
                        {
                            await ms.CopyToAsync(stream, token);
                        }
                    })
                    .Returns(Task.CompletedTask);

            var failedResponse = new FileConvertResult { IsSuccess = false, ErrorMessage = "Conversion failed." };

            _mockServiceDispatcher.Setup(sd => sd.DispatchAsync(It.IsAny<FileConvertRequest>()))
                                  .ReturnsAsync(failedResponse);

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            SetupUserContext(user, "User");

            var result = await _controller.Convert(serviceId, mockFile.Object, "original.txt", "pdf", false);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var jsonResult = JObject.FromObject(badRequestResult.Value);
            Assert.Equal("Грешка при конвертиране на файла: Conversion failed.".Trim(), jsonResult["message"].ToString().Trim());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Service execution failed for ID: {serviceId}. Error: Conversion failed.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Convert_ShouldReturnStatusCode500_WhenConvertedContentIsEmpty()
        {
            var serviceId = ServiceConstants.FileConverterServiceId;
            var testFileContent = Encoding.UTF8.GetBytes("test content");

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("original.txt");
            mockFile.Setup(f => f.Length).Returns(testFileContent.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(testFileContent));
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Callback<Stream, CancellationToken>(async (stream, token) =>
                    {
                        using (var ms = new MemoryStream(testFileContent))
                        {
                            await ms.CopyToAsync(stream, token);
                        }
                    })
                    .Returns(Task.CompletedTask);

            var emptyContentResult = new FileConvertResult
            {
                IsSuccess = true,
                ConvertedFileContent = new byte[0],
                ConvertedFileName = "empty.pdf",
                ContentType = "application/pdf"
            };

            _mockServiceDispatcher.Setup(sd => sd.DispatchAsync(It.IsAny<FileConvertRequest>()))
                                  .ReturnsAsync(emptyContentResult);

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            SetupUserContext(user, "User");

            var result = await _controller.Convert(serviceId, mockFile.Object, "original.txt", "pdf", false);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var jsonResult = JObject.FromObject(statusCodeResult.Value);
            Assert.Equal("Файлът е успешно конвертиран, но не бе получено съдържание за изтегляне.".Trim(), jsonResult["message"].ToString().Trim());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Conversion was successful, but no file content was returned.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Convert_ShouldReturnStatusCode500_WhenResponseIsNotFileConvertResult()
        {
            var serviceId = ServiceConstants.FileConverterServiceId;
            var testFileContent = Encoding.UTF8.GetBytes("test content");

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("original.txt");
            mockFile.Setup(f => f.Length).Returns(testFileContent.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(testFileContent));
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Callback<Stream, CancellationToken>(async (stream, token) =>
                    {
                        using (var ms = new MemoryStream(testFileContent))
                        {
                            await ms.CopyToAsync(stream, token);
                        }
                    })
                    .Returns(Task.CompletedTask);

            var unexpectedResponse = new Mock<MockableBaseServiceResponse>();
            unexpectedResponse.SetupGet(r => r.IsSuccess).Returns(true);

            _mockServiceDispatcher.Setup(sd => sd.DispatchAsync(It.IsAny<FileConvertRequest>()))
                                  .ReturnsAsync(unexpectedResponse.Object);

            var user = new ApplicationUser { Id = "test-user-id", UserName = "testuser" };
            SetupUserContext(user, "User");

            var result = await _controller.Convert(serviceId, mockFile.Object, "original.txt", "pdf", false);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var jsonResult = JObject.FromObject(statusCodeResult.Value);
            Assert.Equal("Вътрешна грешка: Неочакван отговор от услугата.".Trim(), jsonResult["message"].ToString().Trim());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("ServiceDispatcher returned success, but response was not FileConvertResult.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

       
    }
}
