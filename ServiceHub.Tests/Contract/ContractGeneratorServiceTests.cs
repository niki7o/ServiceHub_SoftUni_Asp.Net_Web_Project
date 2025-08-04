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

namespace ServiceHub.Tests.Contract
{
    public class ContractGeneratorServiceTests
    {
        private readonly Mock<ILogger<ContractGeneratorService>> _mockLogger;
        private readonly Mock<IConverter> _mockConverter;
        private readonly ContractGeneratorService _service;

        public ContractGeneratorServiceTests()
        {
            _mockLogger = new Mock<ILogger<ContractGeneratorService>>();
            _mockConverter = new Mock<IConverter>();
            _service = new ContractGeneratorService(_mockLogger.Object, _mockConverter.Object);
        }

        [Fact]
        public async Task GenerateContractAsync_ShouldReturnSuccessResult_OnSuccessfulConversion()
        {
            var request = new ContractGenerateRequestModel
            {
                ContractType = "ТРУДОВ ДОГОВОР",
                PartyA = "Иван Петров",
                PartyB = "Фирма ЕООД",
                ContractDate = DateTime.Parse("2023-01-15"),
                ContractTerms = "ТРУДОВ ДОГОВОР\n\n1. Страни по договора:\n   - **Работодател**: [ПОПЪЛНЕТЕ ТУК]\n   - **Работник**: [ПОПЪЛНЕТЕ ТУК]\n\n2. Предмет на договора:\n   - Описание: __Извършване на дейност__\n\n---",
                AdditionalInfo = "Допълнителна информация тук."
            };

            var expectedPdfBytes = Encoding.UTF8.GetBytes("Mock PDF Content");
            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(expectedPdfBytes);

            var result = await _service.GenerateContractAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedPdfBytes, result.GeneratedFileContent);
            Assert.Contains("ТРУДОВ_ДОГОВОР_Иван_Петров_Фирма_ЕООД_", result.GeneratedFileName);
            Assert.Equal("application/pdf", result.ContentType);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Attempting to generate contract for ТРУДОВ ДОГОВОР between Иван Петров and Фирма ЕООД.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Generated HTML Content")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateContractAsync_ShouldReturnFailureResult_OnConverterException()
        {
            var request = new ContractGenerateRequestModel
            {
                ContractType = "ДОГОВОР ЗА УСЛУГА",
                PartyA = "Страна А",
                PartyB = "Страна Б",
                ContractDate = DateTime.Now,
                ContractTerms = "Some terms",
                AdditionalInfo = ""
            };

            var exceptionMessage = "Converter error message.";
            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Throws(new Exception(exceptionMessage));

            var result = await _service.GenerateContractAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Contains($"Възникна грешка при генериране на договор: {exceptionMessage}", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"General error occurred during contract generation for {request.ContractType}: {exceptionMessage}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

       

        [Fact]
        public void GenerateContractHtml_ShouldNotIncludeAdditionalInfoWhenEmpty()
        {
            var request = new ContractGenerateRequestModel
            {
                ContractType = "Тестов договор",
                PartyA = "А",
                PartyB = "Б",
                ContractDate = DateTime.Now,
                ContractTerms = "Terms.",
                AdditionalInfo = ""
            };

            var method = _service.GetType().GetMethod("GenerateContractHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request });

            Assert.DoesNotContain("<h2>ДОПЪЛНИТЕЛНА ИНФОРМАЦИЯ</h2>", html);
        }

        [Fact]
        public void GenerateContractHtml_ShouldIncludeDateInfo()
        {
            var contractDate = new DateTime(2024, 7, 31);
            var request = new ContractGenerateRequestModel
            {
                ContractType = "Тестов договор",
                PartyA = "А",
                PartyB = "Б",
                ContractDate = contractDate,
                ContractTerms = "Terms.",
                AdditionalInfo = ""
            };

            var method = _service.GetType().GetMethod("GenerateContractHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request });

            var expectedDateString = $"Настоящият договор е сключен на {contractDate:dd.MM.yyyy} г.";
            Assert.True(Regex.IsMatch(html, Regex.Escape(expectedDateString).Replace(@"\s+", @"\s*"), RegexOptions.IgnoreCase | RegexOptions.Singleline));
        }

        [Theory]
        [InlineData("Normal text.", "<p>Normal text.</p>")]
        [InlineData("Text with **bold**.", "<p>Text with <strong>bold</strong>.</p>")]
        [InlineData("Text with __underline__.", "<p>Text with <u>underline</u>.</p>")]
        [InlineData("Text with _italic_.", "<p>Text with <i>italic</i>.</p>")]
        [InlineData("Text with [ПОПЪЛНЕТЕ ТУК].", "<p>Text with <u>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</u>.</p>")]
        [InlineData("Text with [ПОЛЕТА В КВАДРАТНИ СКОБИ].", "<p>Text with <u><strong>ПОЛЕТА В КВАДРАТНИ СКОБИ</strong></u>.</p>")]
        [InlineData("---", "<hr/>")]
        [InlineData("1. Раздел едно", "<h2>1. Раздел едно</h2>")]
        [InlineData("- Елемент от списък", "<ul><li>Елемент от списък</li></ul>")]
        [InlineData("- Елемент 1\n  - Под-елемент 1.1\n  - Под-елемент 1.2\n- Елемент 2", "<ul><li>Елемент 1</li><ul><li>Под-елемент 1.1</li><li>Под-елемент 1.2</li></ul><li>Елемент 2</li></ul>")]
        public void ConvertPlainTextToHtml_ShouldProcessFormattingCorrectly(string plainText, string expectedHtmlSnippet)
        {
            var method = _service.GetType().GetMethod("ConvertPlainTextToHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { plainText });

           
            var normalizedExpected = Regex.Escape(expectedHtmlSnippet).Replace(@"\s+", @"\s*");
            Assert.True(Regex.IsMatch(html, normalizedExpected, RegexOptions.IgnoreCase | RegexOptions.Singleline),
                $"Expected to find '{expectedHtmlSnippet}' in HTML. Actual HTML: {html}");
        }
    }
}
