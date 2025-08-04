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

namespace ServiceHub.Tests.CvGenerator
{
    public class CvGeneratorServiceTests
    {
        private readonly Mock<ILogger<CvGeneratorService>> _mockLogger;
        private readonly Mock<IConverter> _mockConverter;
        private readonly CvGeneratorService _service;

        public CvGeneratorServiceTests()
        {
            _mockLogger = new Mock<ILogger<CvGeneratorService>>();
            _mockConverter = new Mock<IConverter>();
            _service = new CvGeneratorService(_mockLogger.Object, _mockConverter.Object);
        }

        [Fact]
        public async Task GenerateCvAsync_ShouldReturnSuccessResult_OnSuccessfulPdfGeneration()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Джон Доу",
                Email = "john.doe@example.com",
                Phone = "123-456-7890",
                Summary = "Опитен разработчик.",
                Experience = "Работил в компания Х.",
                Education = "Завършил университет Y.",
                Skills = "C#, .NET, SQL"
            };

            var expectedPdfBytes = Encoding.UTF8.GetBytes("Mock PDF Content");
            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(expectedPdfBytes);

            var result = await _service.GenerateCvAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedPdfBytes, result.GeneratedFileContent);
            Assert.Equal("Джон_Доу_CV.pdf", result.GeneratedFileName);
            Assert.Equal("application/pdf", result.ContentType);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"GenerateCvAsync: Започва генериране на CV за {request.Name}.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GenerateCvAsync: Генериран HTML:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GenerateCvAsync: PDF генериран успешно. Дължина:")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateCvAsync_ShouldReturnFailureResult_OnConverterException()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Джон Доу",
                Email = "john.doe@example.com"
            };
            var exceptionMessage = "Simulated PDF conversion error.";
            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Throws(new Exception(exceptionMessage));

            var result = await _service.GenerateCvAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal($"Грешка при генериране на PDF: {exceptionMessage}", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GenerateCvAsync: Грешка при конвертиране на HTML в PDF с DinkToPdf.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateCvAsync_ShouldReturnFailureResult_OnEmptyPdfBytes()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Джон Доу",
                Email = "john.doe@example.com"
            };
            _mockConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>())).Returns(new byte[0]);

            var result = await _service.GenerateCvAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Неуспешно генериране на PDF файл (празен резултат).", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("GenerateCvAsync: DinkToPdf върна празен или null PDF.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public void GenerateCvHtml_ShouldIncludeBasicInfo()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Тест Име",
                Email = "test@example.com",
                Phone = "123-456"
            };

            var method = _service.GetType().GetMethod("GenerateCvHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request });

            Assert.True(Regex.IsMatch(html, $"<h1>{Regex.Escape(request.Name)}</h1>", RegexOptions.IgnoreCase));
            Assert.True(Regex.IsMatch(html, $"<p><i class='fas fa-envelope'></i> {Regex.Escape(request.Email)}</p>", RegexOptions.IgnoreCase));
            Assert.True(Regex.IsMatch(html, $"<p><i class='fas fa-phone'></i> {Regex.Escape(request.Phone)}</p>", RegexOptions.IgnoreCase));
        }

        [Fact]
        public void GenerateCvHtml_ShouldExcludePhoneIfEmpty()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Тест Име",
                Email = "test@example.com",
                Phone = ""
            };

            var method = _service.GetType().GetMethod("GenerateCvHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request });

            Assert.DoesNotContain("<p><i class='fas fa-phone'></i></p>", html);
        }

        [Theory]
        [InlineData("Summary content.", "Профил", "user-circle", "<p class='item-description'>Summary content.</p>")]
        [InlineData("Experience content.", "Професионален Опит", "briefcase", "<p class='item-description'>Experience content.</p>")]
        [InlineData("Education content.", "Образование", "graduation-cap", "<p class='item-description'>Education content.</p>")]
        public void GenerateCvHtml_ShouldIncludeSectionIfContentProvided(string content, string title, string iconClassSuffix, string expectedParagraphHtml)
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Тест",
                Email = "test@test.com",
                Summary = title == "Профил" ? content : null,
                Experience = title == "Професионален Опит" ? content : null,
                Education = title == "Образование" ? content : null,
                Skills = null
            };

            var method = _service.GetType().GetMethod("GenerateCvHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request });

          
            string titlePattern = Regex.Escape($"<h2 class='section-title'><i class='fas fa-{iconClassSuffix}'></i> {title}</h2>")
                                    .Replace(@"\s+", @"\s*"); 

            string paragraphPattern = Regex.Escape(expectedParagraphHtml.Replace(Environment.NewLine, "<br/>"))
                                      .Replace(@"\s+", @"\s*"); 

            Assert.True(Regex.IsMatch(html, titlePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline),
                $"Expected title '{title}' not found in HTML. Actual HTML: {html}");
            Assert.True(Regex.IsMatch(html, paragraphPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline),
                $"Expected paragraph '{expectedParagraphHtml}' not found in HTML. Actual HTML: {html}");
        }

        [Theory]
        [InlineData("Профил", "user-circle")]
        [InlineData("Професионален Опит", "briefcase")]
        [InlineData("Образование", "graduation-cap")]
        public void GenerateCvHtml_ShouldExcludeSectionIfContentEmpty(string title, string iconClassSuffix)
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Тест",
                Email = "test@test.com",
                Summary = title == "Профил" ? "" : null,
                Experience = title == "Професионален Опит" ? "" : null,
                Education = title == "Образование" ? "" : null,
                Skills = null
            };

            var method = _service.GetType().GetMethod("GenerateCvHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request });

            string titlePattern = Regex.Escape($"<h2 class='section-title'><i class='fas fa-{iconClassSuffix}'></i> {title}</h2>")
                                    .Replace(@"\s+", @"\s*"); 

            Assert.False(Regex.IsMatch(html, titlePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline),
                $"Section with title '{title}' was found but should have been excluded. Actual HTML: {html}");
        }

        [Fact]
        public void GenerateCvHtml_ShouldIncludeSkillsCorrectly()
        {
            var request = new CvGenerateRequestModel
            {
                Name = "Тест",
                Email = "test@test.com",
                Skills = "C#, .NET, SQL;JavaScript, HTML"
            };

            var method = _service.GetType().GetMethod("GenerateCvHtml", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = (string)method.Invoke(_service, new object[] { request });

            string normalizedHtml = Regex.Replace(html, @"\s+", "");

            Assert.True(Regex.IsMatch(normalizedHtml, Regex.Replace("<h2 class='section-title'><i class='fas fa-lightbulb'></i> Умения</h2>", @"\s+", ""), RegexOptions.IgnoreCase));
            Assert.Contains(Regex.Replace("<span class='skill-item'>C#</span>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<span class='skill-item'>.NET</span>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<span class='skill-item'>SQL</span>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<span class='skill-item'>JavaScript</span>", @"\s+", ""), normalizedHtml);
            Assert.Contains(Regex.Replace("<span class='skill-item'>HTML</span>", @"\s+", ""), normalizedHtml);
        }

        
    }
}
