using DinkToPdf;
using DinkToPdf.Contracts;
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

namespace ServiceHub.Tests.FinancialCalculator
{
    public class FinancialCalculatorServiceTests
    {
        private readonly Mock<ILogger<FinancialCalculatorService>> _mockLogger;
        private readonly Mock<IConverter> _mockConverter;
        private readonly FinancialCalculatorService _service;

        public FinancialCalculatorServiceTests()
        {
            _mockLogger = new Mock<ILogger<FinancialCalculatorService>>();
            _mockConverter = new Mock<IConverter>();
            _service = new FinancialCalculatorService(_mockLogger.Object, _mockConverter.Object);
        }

        [Fact]
        public async Task CalculateFinancialsAsync_ShouldCalculateROICorrectly_WhenCostOfInvestmentIsPositive()
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

            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(new byte[] { 0x01 });

            var result = await _service.CalculateFinancialsAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(20.0m, result.CalculatedROI); // (1000 / 5000) * 100 = 20
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Attempting financial calculation.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Financial report successfully generated as PDF.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CalculateFinancialsAsync_ShouldSetROIToZero_WhenCostOfInvestmentIsZero()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 1000m,
                CostOfInvestment = 0m,
                Revenues = new List<RevenueItem>(),
                Expenses = new List<ExpenseItem>(),
                GrowthRatePercentage = 0,
                ForecastPeriodMonths = 0
            };

            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(new byte[] { 0x01 });

            var result = await _service.CalculateFinancialsAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(0m, result.CalculatedROI);
        }

        [Fact]
        public async Task CalculateFinancialsAsync_ShouldCalculateTotalsAndNetProfitLossCorrectly()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 0,
                CostOfInvestment = 1,
                Revenues = new List<RevenueItem>
                {
                    new RevenueItem { Description = "Продажби", Amount = 1000m },
                    new RevenueItem { Description = "Лихви", Amount = 50m }
                },
                Expenses = new List<ExpenseItem>
                {
                    new ExpenseItem { Description = "Наем", Amount = 200m },
                    new ExpenseItem { Description = "Заплати", Amount = 300m }
                },
                GrowthRatePercentage = 0,
                ForecastPeriodMonths = 0
            };

            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(new byte[] { 0x01 });

            var result = await _service.CalculateFinancialsAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(1050m, result.TotalRevenues);
            Assert.Equal(500m, result.TotalExpenses);
            Assert.Equal(550m, result.NetProfitLoss); // 1050 - 500 = 550
        }

        [Fact]
        public async Task CalculateFinancialsAsync_ShouldForecastCorrectly()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 0,
                CostOfInvestment = 1,
                Revenues = new List<RevenueItem>
                {
                    new RevenueItem { Description = "Начални приходи", Amount = 100m }
                },
                Expenses = new List<ExpenseItem>
                {
                    new ExpenseItem { Description = "Начални разходи", Amount = 50m }
                },
                GrowthRatePercentage = 10m, // 10% growth
                ForecastPeriodMonths = 2 // 2 months
            };

            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(new byte[] { 0x01 });

            var result = await _service.CalculateFinancialsAsync(request);

            Assert.True(result.IsSuccess);
            // Month 1: Revenues = 100 * 1.1 = 110, Expenses = 50 * 1.1 = 55
            // Month 2: Revenues = 110 * 1.1 = 121, Expenses = 55 * 1.1 = 60.5
            Assert.Equal(121.0m, result.ForecastedRevenues);
            Assert.Equal(60.5m, result.ForecastedExpenses);
            Assert.Equal(60.5m, result.ForecastedNetProfitLoss); // 121 - 60.5 = 60.5
        }

        [Fact]
        public async Task CalculateFinancialsAsync_ShouldReturnFailureResult_OnException()
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

            var exceptionMessage = "Simulated service error.";
            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Throws(new Exception(exceptionMessage));

            var result = await _service.CalculateFinancialsAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal($"Възникна грешка при изчисляване: {exceptionMessage}", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"General error occurred during financial calculation: {exceptionMessage}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public void GenerateFinancialReportHtml_ShouldIncludeAllSections()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 1000m,
                CostOfInvestment = 5000m,
                Revenues = new List<RevenueItem> { new RevenueItem { Description = "Продажби", Amount = 1000m } },
                Expenses = new List<ExpenseItem> { new ExpenseItem { Description = "Наем", Amount = 200m } },
                GrowthRatePercentage = 10m,
                ForecastPeriodMonths = 2,
                Notes = "Тестови бележки."
            };
            var result = new FinancialCalculationResult
            {
                CalculatedROI = 20.0m,
                TotalRevenues = 1000m,
                TotalExpenses = 200m,
                NetProfitLoss = 800m,
                ForecastedRevenues = 1210m,
                ForecastedExpenses = 242m,
                ForecastedNetProfitLoss = 968m
            };

            var method = _service.GetType().GetMethod("GenerateFinancialReportHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, result });

            // Normalize HTML for robust comparison
            var normalizedHtml = Regex.Replace(html, @"\s+", "");

            Assert.Contains(Regex.Replace("<h1>Финансов Отчет</h1>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<h2>1. Анализ на възвръщаемост на инвестициите (ROI)</h2>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>НетнапечалбазаROI:<strong>{request.NetProfitForROI:F2}лв.</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Ценанаинвестицията:<strong>{request.CostOfInvestment:F2}лв.</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<strong>ИзчисленROI:<spanstyle='color:#28a745;'>{result.CalculatedROI:F2}%</span></strong>", @"\s+", ""), normalizedHtml);

            Assert.Contains(Regex.Replace("<h2>2. Бюджет / Отчет за приходи и разходи</h2>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<h3>Приходи</h3>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<table><thead><tr><th>Описание</th><th>Сума</th></tr></thead><tbody><tr><td>Продажби</td><td>1000.00лв.</td></tr></tbody></table>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<h3>Разходи</h3>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<table><thead><tr><th>Описание</th><th>Сума</th></tr></thead><tbody><tr><td>Наем</td><td>200.00лв.</td></tr></tbody></table>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<tdclass='label'>Общиприходи:</td><tdclass='value'>{result.TotalRevenues:F2}лв.</td>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<tdclass='label'>Общиразходи:</td><tdclass='value'>{result.TotalExpenses:F2}лв.</td>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<tdclass='label'>Нетнапечалба/загуба:</td><tdclass='value net-profit'>{result.NetProfitLoss:F2}лв.</td>", @"\s+", ""), normalizedHtml);

            Assert.Contains(Regex.Replace("<h2>3. Прогноза</h2>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Процентнарастеж(напериод):<strong>{request.GrowthRatePercentage:F2}%</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Периоднапрогноза:<strong>{request.ForecastPeriodMonths}месеца</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<tdclass='label'>Прогнозираниприходи:</td><tdclass='value'>{result.ForecastedRevenues:F2}лв.</td>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<tdclass='label'>Прогнозираниразходи:</td><tdclass='value'>{result.ForecastedExpenses:F2}лв.</td>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<tdclass='label'>Прогнозирананетнапечалба/загуба:</td><tdclass='value net-profit'>{result.ForecastedNetProfitLoss:F2}лв.</td>", @"\s+", ""), normalizedHtml);

            Assert.Contains(Regex.Replace("<h3>Бележки:</h3>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>{request.Notes.Replace(Environment.NewLine, "<br/>")}</p>", @"\s+", ""), normalizedHtml);
        }

        [Fact]
        public void GenerateFinancialReportHtml_ShouldExcludeNotesSectionIfNotesEmpty()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 0,
                CostOfInvestment = 1,
                Revenues = new List<RevenueItem>(),
                Expenses = new List<ExpenseItem>(),
                GrowthRatePercentage = 0,
                ForecastPeriodMonths = 0,
                Notes = ""
            };
            var result = new FinancialCalculationResult();

            var method = _service.GetType().GetMethod("GenerateFinancialReportHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, result });

            Assert.DoesNotContain("<h3>Бележки:</h3>", html);
        }

        [Fact]
        public void GenerateFinancialReportHtml_ShouldDisplayNoRevenuesMessageWhenEmpty()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 0,
                CostOfInvestment = 1,
                Revenues = new List<RevenueItem>(),
                Expenses = new List<ExpenseItem>(),
                GrowthRatePercentage = 0,
                ForecastPeriodMonths = 0
            };
            var result = new FinancialCalculationResult();

            var method = _service.GetType().GetMethod("GenerateFinancialReportHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, result });

            Assert.Contains("<p>Няма въведени приходи.</p>", html);
            Assert.DoesNotContain("<table><thead><tr><th>Описание</th><th>Сума</th></tr></thead><tbody>", html);
        }

        [Fact]
        public void GenerateFinancialReportHtml_ShouldDisplayNoExpensesMessageWhenEmpty()
        {
            var request = new FinancialCalculatorRequestModel
            {
                NetProfitForROI = 0,
                CostOfInvestment = 1,
                Revenues = new List<RevenueItem>(),
                Expenses = new List<ExpenseItem>(),
                GrowthRatePercentage = 0,
                ForecastPeriodMonths = 0
            };
            var result = new FinancialCalculationResult();

            var method = _service.GetType().GetMethod("GenerateFinancialReportHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, result });

            Assert.Contains("<p>Няма въведени разходи.</p>", html);
            Assert.DoesNotContain("<table><thead><tr><th>Описание</th><th>Сума</th></tr></thead><tbody>", html);
        }
    }
}
