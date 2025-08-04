//using Xunit;
//using Moq;
//using ServiceHub.Controllers;
//using ServiceHub.Services.Interfaces;
//using ServiceHub.Core.Models.Service.FileConverter;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using System;
//using System.IO;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using ServiceHub.Common;
//using Microsoft.Extensions.Primitives;
//using System.Collections.Generic;
//using System.Threading;
//using ServiceHub.Core.Models;
//using Newtonsoft.Json.Linq;

//namespace ServiceHub.Tests.FileConverter
//{
//    public class MockableBaseServiceResponse : BaseServiceResponse
//    {
//        public override bool IsSuccess { get; set; }
//        public override string ErrorMessage { get; set; } = string.Empty;
//    }

//    public class FileConverterControllerTests
//    {
//        private readonly Mock<ILogger<FileConverterController>> _mockLogger;
//        private readonly Mock<IServiceDispatcher> _mockServiceDispatcher;
//        private readonly FileConverterController _controller;

//        public FileConverterControllerTests()
//        {
//            _mockLogger = new Mock<ILogger<FileConverterController>>();
//            _mockServiceDispatcher = new Mock<IServiceDispatcher>();

//            _controller = new FileConverterController(_mockLogger.Object, _mockServiceDispatcher.Object);

//            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
//            _controller.TempData = tempData;
//        }

//        [Fact]
//        public void Index_ShouldReturnFileConverterFormView()
//        {
//            var result = _controller.Index();

//            var viewResult = Assert.IsType<ViewResult>(result);
//            Assert.Equal("~/Views/Service/_FileConverterForm.cshtml", viewResult.ViewName);
//            Assert.Equal(ServiceConstants.FileConverterServiceId, viewResult.ViewData["ServiceId"]);
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Information,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Loading File Converter Index view with ServiceId: {ServiceConstants.FileConverterServiceId}")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }

        
//        [Fact]
//        public async Task Convert_ShouldReturnBadRequest_WhenServiceIdIsInvalid()
//        {
//            var result = await _controller.Convert(Guid.Empty, Mock.Of<IFormFile>(), null, "pdf", false);

//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            var jsonResult = JObject.FromObject(badRequestResult.Value);
//            Assert.Equal("Идентификаторът на услугата е задължителен.", jsonResult["message"].ToString());
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Warning,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Липсва ServiceId в заявката.")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }

//        [Fact]
//        public async Task Convert_ShouldReturnBadRequest_WhenServiceIdDoesNotMatchConstant()
//        {
//            var wrongServiceId = Guid.NewGuid();
//            var result = await _controller.Convert(wrongServiceId, Mock.Of<IFormFile>(), null, "pdf", false);

//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            var jsonResult = JObject.FromObject(badRequestResult.Value);
//            Assert.Equal("Невалиден идентификатор на услугата.", jsonResult["message"].ToString());
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Warning,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Невалиден ServiceId: {wrongServiceId}. Очакван: {ServiceConstants.FileConverterServiceId}")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }

//        [Fact]
//        public async Task Convert_ShouldReturnBadRequest_WhenFileContentIsMissing()
//        {
//            var serviceId = ServiceConstants.FileConverterServiceId;
//            var result = await _controller.Convert(serviceId, null, null, "pdf", false);

//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            var jsonResult = JObject.FromObject(badRequestResult.Value);
//            Assert.Equal("Моля, изберете файл за конвертиране.", jsonResult["message"].ToString());
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Warning,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Липсва съдържание на файл.")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }

//        [Fact]
//        public async Task Convert_ShouldReturnBadRequest_WhenTargetFormatIsMissing()
//        {
//            var serviceId = ServiceConstants.FileConverterServiceId;
//            var mockFile = new Mock<IFormFile>();
//            mockFile.Setup(f => f.FileName).Returns("test.txt");
//            mockFile.Setup(f => f.Length).Returns(10);
//            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

//            var result = await _controller.Convert(serviceId, mockFile.Object, null, "", false);

//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            var jsonResult = JObject.FromObject(badRequestResult.Value);
//            Assert.Equal("Моля, изберете целеви формат.", jsonResult["message"].ToString());
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Warning,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Липсва целеви формат.")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }

//        [Fact]
//        public async Task Convert_ShouldReturnBadRequest_WhenConversionFails()
//        {
//            var serviceId = ServiceConstants.FileConverterServiceId;
//            var testFileContent = Encoding.UTF8.GetBytes("test content");

//            var mockFile = new Mock<IFormFile>();
//            mockFile.Setup(f => f.FileName).Returns("original.txt");
//            mockFile.Setup(f => f.Length).Returns(testFileContent.Length);
//            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
//                    .Callback<Stream, CancellationToken>(async (stream, token) =>
//                    {
//                        using (var ms = new MemoryStream(testFileContent))
//                        {
//                            await ms.CopyToAsync(stream, token);
//                        }
//                    })
//                    .Returns(Task.CompletedTask);

//            var failedResponse = new FileConvertResult { IsSuccess = false, ErrorMessage = "Conversion failed." };

//            _mockServiceDispatcher.Setup(sd => sd.DispatchAsync(It.IsAny<FileConvertRequest>()))
//                                  .ReturnsAsync(failedResponse);

//            var result = await _controller.Convert(serviceId, mockFile.Object, "original.txt", "pdf", false);

//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            var jsonResult = JObject.FromObject(badRequestResult.Value);
//            Assert.Equal("Грешка при конвертиране на файла: Conversion failed.", jsonResult["message"].ToString());
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Error,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Service execution failed for ID: {serviceId}. Error: Conversion failed.")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }

//        [Fact]
//        public async Task Convert_ShouldReturnStatusCode500_WhenConvertedContentIsEmpty()
//        {
//            var serviceId = ServiceConstants.FileConverterServiceId;
//            var testFileContent = Encoding.UTF8.GetBytes("test content");

//            var mockFile = new Mock<IFormFile>();
//            mockFile.Setup(f => f.FileName).Returns("original.txt");
//            mockFile.Setup(f => f.Length).Returns(testFileContent.Length);
//            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
//                    .Callback<Stream, CancellationToken>(async (stream, token) =>
//                    {
//                        using (var ms = new MemoryStream(testFileContent))
//                        {
//                            await ms.CopyToAsync(stream, token);
//                        }
//                    })
//                    .Returns(Task.CompletedTask);

//            var emptyContentResult = new FileConvertResult
//            {
//                IsSuccess = true,
//                ConvertedFileContent = new byte[0],
//                ConvertedFileName = "empty.pdf",
//                ContentType = "application/pdf"
//            };

//            _mockServiceDispatcher.Setup(sd => sd.DispatchAsync(It.IsAny<FileConvertRequest>()))
//                                  .ReturnsAsync(emptyContentResult);

//            var result = await _controller.Convert(serviceId, mockFile.Object, "original.txt", "pdf", false);

//            var statusCodeResult = Assert.IsType<ObjectResult>(result);
//            Assert.Equal(500, statusCodeResult.StatusCode);
//            var jsonResult = JObject.FromObject(statusCodeResult.Value);
//            Assert.Equal("Файлът е успешно конвертиран, но не бе получено съдържание за изтегляне.", jsonResult["message"].ToString());
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Warning,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Conversion was successful, but no file content was returned.")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }

//        [Fact]
//        public async Task Convert_ShouldReturnStatusCode500_WhenResponseIsNotFileConvertResult()
//        {
//            var serviceId = ServiceConstants.FileConverterServiceId;
//            var testFileContent = Encoding.UTF8.GetBytes("test content");

//            var mockFile = new Mock<IFormFile>();
//            mockFile.Setup(f => f.FileName).Returns("original.txt");
//            mockFile.Setup(f => f.Length).Returns(testFileContent.Length);
//            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
//                    .Callback<Stream, CancellationToken>(async (stream, token) =>
//                    {
//                        using (var ms = new MemoryStream(testFileContent))
//                        {
//                            await ms.CopyToAsync(stream, token);
//                        }
//                    })
//                    .Returns(Task.CompletedTask);

//            var unexpectedResponse = new Mock<MockableBaseServiceResponse>();
//            unexpectedResponse.SetupGet(r => r.IsSuccess).Returns(true);

//            _mockServiceDispatcher.Setup(sd => sd.DispatchAsync(It.IsAny<FileConvertRequest>()))
//                                  .ReturnsAsync(unexpectedResponse.Object);

//            var result = await _controller.Convert(serviceId, mockFile.Object, "original.txt", "pdf", false);

//            var statusCodeResult = Assert.IsType<ObjectResult>(result);
//            Assert.Equal(500, statusCodeResult.StatusCode);
//            var jsonResult = JObject.FromObject(statusCodeResult.Value);
//            Assert.Equal("Вътрешна грешка: Неочакван отговор от услугата.", jsonResult["message"].ToString());
//            _mockLogger.Verify(
//                x => x.Log(
//                    LogLevel.Error,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("ServiceDispatcher returned success, but response was not FileConvertResult.")),
//                    It.IsAny<Exception>(),
//                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
//                Times.Once
//            );
//        }
//    }
//}
