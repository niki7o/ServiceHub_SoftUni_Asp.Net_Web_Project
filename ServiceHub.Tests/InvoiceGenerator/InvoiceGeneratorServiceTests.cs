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

namespace ServiceHub.Tests.InvoiceGenerator
{
    public class InvoiceGeneratorServiceTests
    {
        private readonly Mock<ILogger<InvoiceGeneratorService>> _mockLogger;
        private readonly Mock<IConverter> _mockConverter;
        private readonly InvoiceGeneratorService _service;

        public InvoiceGeneratorServiceTests()
        {
            _mockLogger = new Mock<ILogger<InvoiceGeneratorService>>();
            _mockConverter = new Mock<IConverter>();
            _service = new InvoiceGeneratorService(_mockLogger.Object, _mockConverter.Object);
        }

        [Fact]
        public async Task GenerateInvoiceAsync_ShouldCalculateTotalsCorrectly()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-001",
                IssueDate = DateTime.Now,
                SellerName = "Продавач ООД",
                BuyerName = "Купувач ЕООД",
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { Description = "Продукт А", Quantity = 2, UnitPrice = 100m },
                    new InvoiceItem { Description = "Услуга Б", Quantity = 1, UnitPrice = 50m }
                },
                DiscountPercentage = 10m, 
                TaxRatePercentage = 20m, 
                PaymentMethod = "Банков превод",
                Notes = "Тестови бележки."
            };

            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(new byte[] { 0x01 });

            var result = await _service.GenerateInvoiceAsync(request);

            Assert.True(result.IsSuccess);
         

           
            var subtotalField = result.GetType().GetField("_subtotal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var discountAmountField = result.GetType().GetField("_discountAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var taxAmountField = result.GetType().GetField("_taxAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var totalAmountField = result.GetType().GetField("_totalAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

           
            var generateInvoiceHtmlMethod = _service.GetType().GetMethod("GenerateInvoiceHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            decimal subtotal = request.Items.Sum(item => item.Quantity * item.UnitPrice);
            decimal discountAmount = subtotal * (request.DiscountPercentage / 100);
            decimal taxableAmount = subtotal - discountAmount;
            decimal taxAmount = taxableAmount * (request.TaxRatePercentage / 100);
            decimal totalAmount = taxableAmount + taxAmount;


            Assert.Equal(250m, subtotal);
            Assert.Equal(25m, discountAmount);
            Assert.Equal(45m, taxAmount);
            Assert.Equal(270m, totalAmount);

            Assert.Contains($"Invoice_{request.InvoiceNumber}", result.GeneratedFileName);
            Assert.Equal("application/pdf", result.ContentType);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Attempting to generate invoice {request.InvoiceNumber}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Generated HTML Content (first 500 chars):")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Invoice {request.InvoiceNumber} successfully generated as PDF.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateInvoiceAsync_ShouldReturnFailureResult_OnConverterException()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-002",
                IssueDate = DateTime.Now,
                SellerName = "Продавач ООД",
                BuyerName = "Купувач ЕООД",
                Items = new List<InvoiceItem>(),
                DiscountPercentage = 0,
                TaxRatePercentage = 0
            };
            var exceptionMessage = "Simulated PDF conversion error.";
            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Throws(new Exception(exceptionMessage));

            var result = await _service.GenerateInvoiceAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal($"Възникна грешка при генериране на фактура: {exceptionMessage}", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"General error occurred during invoice generation for {request.InvoiceNumber}: {exceptionMessage}")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public void GenerateInvoiceHtml_ShouldIncludeHeaderAndDetails()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-TEST-001",
                IssueDate = new DateTime(2024, 7, 31),
                SellerName = "Тест Продавач",
                SellerAddress = "Улица 1, Град",
                SellerEIK = "123456789",
                SellerMOL = "Иван Иванов",
                BuyerName = "Тест Купувач",
                BuyerAddress = "Булевард 2, Село",
                BuyerEIK = "987654321",
                BuyerMOL = "Петър Петров",
                Items = new List<InvoiceItem>(),
                DiscountPercentage = 0,
                TaxRatePercentage = 0,
                PaymentMethod = "В брой",
                Notes = "Някои бележки."
            };
            decimal subtotal = 0, discountAmount = 0, taxAmount = 0, totalAmount = 0;

            var method = _service.GetType().GetMethod("GenerateInvoiceHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, subtotal, discountAmount, taxAmount, totalAmount });

            var normalizedHtml = Regex.Replace(html, @"\s+", "");

            Assert.Contains(Regex.Replace("<h1>ФАКТУРА</h1>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Номер:<strong>{request.InvoiceNumber}</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Датанаиздаване:<strong>{request.IssueDate:dd.MM.yyyy}</strong></p>", @"\s+", ""), normalizedHtml);

            Assert.Contains(Regex.Replace("<h3>Продавач</h3>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p><strong>{request.SellerName}</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Адрес:{request.SellerAddress}</p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>ЕИК/БУЛСТАТ:{request.SellerEIK}</p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>МОЛ:{request.SellerMOL}</p>", @"\s+", ""), normalizedHtml);

            Assert.Contains(Regex.Replace("<h3>Купувач</h3>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p><strong>{request.BuyerName}</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Адрес:{request.BuyerAddress}</p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>ЕИК/БУЛСТАТ:{request.BuyerEIK}</p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>МОЛ:{request.BuyerMOL}</p>", @"\s+", ""), normalizedHtml);
        }

        [Fact]
        public void GenerateInvoiceHtml_ShouldExcludeOptionalSellerBuyerFieldsIfEmpty()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-TEST-002",
                IssueDate = DateTime.Now,
                SellerName = "Продавач",
                SellerAddress = "",
                SellerEIK = "",
                SellerMOL = "",
                BuyerName = "Купувач",
                BuyerAddress = "",
                BuyerEIK = "",
                BuyerMOL = "",
                Items = new List<InvoiceItem>(),
                DiscountPercentage = 0,
                TaxRatePercentage = 0
            };
            decimal subtotal = 0, discountAmount = 0, taxAmount = 0, totalAmount = 0;

            var method = _service.GetType().GetMethod("GenerateInvoiceHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, subtotal, discountAmount, taxAmount, totalAmount });

            Assert.DoesNotContain("Адрес: ", html);
            Assert.DoesNotContain("ЕИК/БУЛСТАТ: ", html);
            Assert.DoesNotContain("МОЛ: ", html);
        }

        [Fact]
        public void GenerateInvoiceHtml_ShouldListItemsCorrectly()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-TEST-003",
                IssueDate = DateTime.Now,
                SellerName = "С",
                BuyerName = "К",
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { Description = "Item 1", Quantity = 1.5m, UnitPrice = 10.0m },
                    new InvoiceItem { Description = "Item 2", Quantity = 3m, UnitPrice = 5.25m }
                },
                DiscountPercentage = 0,
                TaxRatePercentage = 0
            };
            decimal subtotal = 1.5m * 10m + 3m * 5.25m; // 15 + 15.75 = 30.75
            decimal discountAmount = 0, taxAmount = 0, totalAmount = subtotal;

            var method = _service.GetType().GetMethod("GenerateInvoiceHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, subtotal, discountAmount, taxAmount, totalAmount });

            var normalizedHtml = Regex.Replace(html, @"\s+", "");

            Assert.Contains(Regex.Replace("<th>Описание</th><th>Количество</th><th>Ед.цена</th><th>Общо</th>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<tr><td>Item1</td><td>1.50</td><td>10.00лв.</td><td>15.00лв.</td></tr>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<tr><td>Item2</td><td>3.00</td><td>5.25лв.</td><td>15.75лв.</td></tr>", @"\s+", ""), normalizedHtml);
        }

        [Fact]
        public void GenerateInvoiceHtml_ShouldIncludeTotalsSectionCorrectly()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-TEST-004",
                IssueDate = DateTime.Now,
                SellerName = "С",
                BuyerName = "К",
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { Description = "Item", Quantity = 1, UnitPrice = 100m }
                },
                DiscountPercentage = 5m,
                TaxRatePercentage = 10m
            };
            decimal subtotal = 100m;
            decimal discountAmount = 5m; 
            decimal taxableAmount = 95m; 
            decimal taxAmount = 9.5m; 
            decimal totalAmount = 104.5m; 

            var method = _service.GetType().GetMethod("GenerateInvoiceHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, subtotal, discountAmount, taxAmount, totalAmount });

            var normalizedHtml = Regex.Replace(html, @"\s+", "");

            Assert.Contains(Regex.Replace($"<span>Междиннасума:</span><span>{subtotal:F2}лв.</span>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<span>Отстъпка({request.DiscountPercentage:F2}%):</span><span>-{discountAmount:F2}лв.</span>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<span>Данък({request.TaxRatePercentage:F2}%):</span><span>+{taxAmount:F2}лв.</span>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<span>Общасумазаплащане:</span><span>{totalAmount:F2}лв.</span>", @"\s+", ""), normalizedHtml);
        }

        [Fact]
        public void GenerateInvoiceHtml_ShouldIncludePaymentMethodAndNotesIfProvided()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-TEST-005",
                IssueDate = DateTime.Now,
                SellerName = "С",
                BuyerName = "К",
                Items = new List<InvoiceItem>(),
                DiscountPercentage = 0,
                TaxRatePercentage = 0,
                PaymentMethod = "Кредитна карта",
                Notes = "Специални инструкции за доставка."
            };
            decimal subtotal = 0, discountAmount = 0, taxAmount = 0, totalAmount = 0;

            var method = _service.GetType().GetMethod("GenerateInvoiceHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, subtotal, discountAmount, taxAmount, totalAmount });

            var normalizedHtml = Regex.Replace(html, @"\s+", "");

            Assert.Contains(Regex.Replace($"<p>Начиннаплащане:<strong>{request.PaymentMethod}</strong></p>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace($"<p>Бележки:{request.Notes}</p>", @"\s+", ""), normalizedHtml);
        }

        [Fact]
        public void GenerateInvoiceHtml_ShouldExcludePaymentMethodAndNotesIfEmpty()
        {
            var request = new InvoiceGenerateRequestModel
            {
                InvoiceNumber = "INV-TEST-006",
                IssueDate = DateTime.Now,
                SellerName = "С",
                BuyerName = "К",
                Items = new List<InvoiceItem>(),
                DiscountPercentage = 0,
                TaxRatePercentage = 0,
                PaymentMethod = "",
                Notes = ""
            };
            decimal subtotal = 0, discountAmount = 0, taxAmount = 0, totalAmount = 0;

            var method = _service.GetType().GetMethod("GenerateInvoiceHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request, subtotal, discountAmount, taxAmount, totalAmount });

            Assert.DoesNotContain("Начин на плащане:", html);
            Assert.DoesNotContain("Бележки:", html);
        }
    }
}
