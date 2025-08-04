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

namespace ServiceHub.Tests.InvoiceGenerator
{
    public class InvoiceGeneratorControllerTests
    {
        private readonly Mock<IInvoiceGeneratorService> _mockInvoiceGeneratorService;
        private readonly Mock<ILogger<InvoiceGeneratorController>> _mockLogger;
        private readonly InvoiceGeneratorController _controller;

        public InvoiceGeneratorControllerTests()
        {
            _mockInvoiceGeneratorService = new Mock<IInvoiceGeneratorService>();
            _mockLogger = new Mock<ILogger<InvoiceGeneratorController>>();
            _controller = new InvoiceGeneratorController(_mockInvoiceGeneratorService.Object, _mockLogger.Object);
        }

        [Fact]
        public void InvoiceGeneratorForm_ReturnsViewResult_WithCorrectViewPathAndServiceId()
        {
            var result = _controller.InvoiceGeneratorForm();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_InvoiceGeneratorForm.cshtml", viewResult.ViewName);
            Assert.Equal("B422F89B-E7A3-4130-B899-7B56010007E0", viewResult.ViewData["ServiceId"]);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Serving InvoiceGeneratorForm view.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateInvoice_ReturnsFileResult_OnSuccess()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-SUCCESS",
                IssueDate = DateTime.Now,
                SellerName = "Продавач",
                BuyerName = "Купувач",
                Items = new List<InvoiceItem> { new InvoiceItem { Description = "Test Item", Quantity = 1, UnitPrice = 10m } },
                DiscountPercentage = 0,
                TaxRatePercentage = 0
            };
            var generatedPdfContent = new byte[] { 0x01, 0x02, 0x03 };
            var generatedFileName = $"Invoice_{request.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf"; 
            var contentType = "application/pdf";

            _mockInvoiceGeneratorService
                .Setup(s => s.GenerateInvoiceAsync(It.IsAny<InvoiceGenerateRequestModel>()))
                .ReturnsAsync(new InvoiceGenerateResult
                {
                    IsSuccess = true,
                    GeneratedFileContent = generatedPdfContent,
                    GeneratedFileName = generatedFileName,
                    ContentType = contentType
                });

            var result = await _controller.GenerateInvoice(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(generatedPdfContent, fileResult.FileContents);
            Assert.Contains($"Invoice_{request.InvoiceNumber}", fileResult.FileDownloadName); 
            Assert.Equal(contentType, fileResult.ContentType);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming InvoiceGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Received invoice generation request for invoice number {request.InvoiceNumber}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Invoice {request.InvoiceNumber} successfully generated. File: {generatedFileName}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateInvoice_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("InvoiceNumber", "Invoice number is required.");

            var request = new InvoiceGenerateRequestModel();

            var result = await _controller.GenerateInvoice(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(badRequestResult.Value));

            Assert.NotNull(errorResponse);
            Assert.True(errorResponse.ContainsKey("errors"));
            Assert.True(errorResponse.ContainsKey("message"));

            var errors = JsonConvert.DeserializeObject<List<string>>(errorResponse["errors"].ToString());
            Assert.Contains("Invoice number is required.", errors);

            var message = errorResponse["message"].ToString();
            Assert.Equal("Въведените данни са невалидни. Моля, проверете всички полета.", message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming InvoiceGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Model validation failed for InvoiceGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateInvoice_ReturnsBadRequest_WhenServiceFails()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-FAIL",
                IssueDate = DateTime.Now,
                SellerName = "Продавач",
                BuyerName = "Купувач",
                Items = new List<InvoiceItem>()
            };
            var errorMessage = "Service failed to generate invoice.";

            _mockInvoiceGeneratorService
                .Setup(s => s.GenerateInvoiceAsync(It.IsAny<InvoiceGenerateRequestModel>()))
                .ReturnsAsync(new InvoiceGenerateResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage
                });

            var result = await _controller.GenerateInvoice(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming InvoiceGenerateRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Received invoice generation request for invoice number {request.InvoiceNumber}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Failed to generate invoice {request.InvoiceNumber}. Error: {errorMessage}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }
    }
}
