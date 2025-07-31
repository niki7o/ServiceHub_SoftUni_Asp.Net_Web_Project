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

namespace ServiceHub.Tests.FinancialCalculator
{
    public class FinancialCalculatorControllerTests
    {
        private readonly Mock<IFinancialCalculatorService> _mockFinancialCalculatorService;
        private readonly Mock<ILogger<FinancialCalculatorController>> _mockLogger;
        private readonly FinancialCalculatorController _controller;

        public FinancialCalculatorControllerTests()
        {
            _mockFinancialCalculatorService = new Mock<IFinancialCalculatorService>();
            _mockLogger = new Mock<ILogger<FinancialCalculatorController>>();
            _controller = new FinancialCalculatorController(_mockFinancialCalculatorService.Object, _mockLogger.Object);
        }

        [Fact]
        public void CalculatorForm_ReturnsViewResult_WithCorrectViewPathAndServiceId()
        {
            var result = _controller.CalculatorForm();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("~/Views/Service/_FinancialCalculatorForm.cshtml", viewResult.ViewName);
            Assert.Equal(FinancialCalculatorController.FinancialCalculatorId.ToString(), viewResult.ViewData["ServiceId"]);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Serving FinancialCalculatorForm view.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CalculateFinancials_ReturnsFileResult_OnSuccess()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 1000m,
                CostOfInvestment = 5000m,
                Revenues = new List<RevenueItem>(),
                Expenses = new List<ExpenseItem>(),
                GrowthRatePercentage = 0,
                ForecastPeriodMonths = 0
            };
            var generatedPdfContent = new byte[] { 0x01, 0x02, 0x03 };
            var generatedFileName = "FinancialReport_20240731_120000.pdf"; // Example filename
            var contentType = "application/pdf";

            _mockFinancialCalculatorService
                .Setup(s => s.CalculateFinancialsAsync(It.IsAny<FinancialCalculatorRequestModel>()))
                .ReturnsAsync(new FinancialCalculationResult
                {
                    IsSuccess = true,
                    GeneratedFileContent = generatedPdfContent,
                    GeneratedFileName = generatedFileName,
                    ContentType = contentType
                });

            var result = await _controller.CalculateFinancials(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal(generatedPdfContent, fileResult.FileContents);
            // Assert.Contains is used here because the filename includes a dynamic timestamp.
            Assert.Contains("FinancialReport_", fileResult.FileDownloadName);
            Assert.Equal(contentType, fileResult.ContentType);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming FinancialCalculatorRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Received financial calculation request.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Financial report successfully generated. File: {generatedFileName}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CalculateFinancials_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("CostOfInvestment", "Cost of investment is required.");

            var request = new FinancialCalculatorRequestModel();

            var result = await _controller.CalculateFinancials(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                JsonConvert.SerializeObject(badRequestResult.Value));

            Assert.NotNull(errorResponse);
            Assert.True(errorResponse.ContainsKey("errors"));
            Assert.True(errorResponse.ContainsKey("message"));

            var errors = JsonConvert.DeserializeObject<List<string>>(errorResponse["errors"].ToString());
            Assert.Contains("Cost of investment is required.", errors);

            var message = errorResponse["message"].ToString();
            Assert.Equal("Въведените данни са невалидни. Моля, проверете всички полета.", message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming FinancialCalculatorRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Model validation failed for FinancialCalculatorRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CalculateFinancials_ReturnsBadRequest_WhenServiceFails()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 1000m,
                CostOfInvestment = 5000m,
                Revenues = new List<RevenueItem>(),
                Expenses = new List<ExpenseItem>(),
                GrowthRatePercentage = 0,
                ForecastPeriodMonths = 0
            };
            var errorMessage = "Service failed to calculate financials.";

            _mockFinancialCalculatorService
                .Setup(s => s.CalculateFinancialsAsync(It.IsAny<FinancialCalculatorRequestModel>()))
                .ReturnsAsync(new FinancialCalculationResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage
                });

            var result = await _controller.CalculateFinancials(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Incoming FinancialCalculatorRequestModel")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Received financial calculation request.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Failed to generate financial report. Error: {errorMessage}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }
    }
}
