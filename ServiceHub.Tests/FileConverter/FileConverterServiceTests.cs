using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceHub.Common;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Services.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xceed.Words.NET;
using OfficeOpenXml;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;


namespace ServiceHub.Tests.FileConverter
{
    public class NonFileConvertRequest : BaseServiceRequest
    {
    }

    public class FileConverterServiceTests
    {
        private readonly Mock<ILogger<FileConverterService>> _mockLogger;
        private readonly Mock<IConverter> _mockPdfConverter;
        private readonly FileConverterService _fileConverterService;

        public FileConverterServiceTests()
        {
            _mockLogger = new Mock<ILogger<FileConverterService>>();
            _mockPdfConverter = new Mock<IConverter>();
            _fileConverterService = new FileConverterService(_mockLogger.Object, _mockPdfConverter.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnInvalidRequestType_WhenRequestIsNotFileConvertRequest()
        {
            var invalidRequest = new NonFileConvertRequest { ServiceId = Guid.NewGuid() };

            var result = await _fileConverterService.ExecuteAsync(invalidRequest);

            Assert.False(result.IsSuccess);
            Assert.Equal("Невалиден тип заявка за услугата за конвертиране на файлове.", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid request type for FileConverterService. Expected FileConvertRequest.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldConvertTxtToDocx()
        {
            var textContent = "Hello, this is a test text.";
            var originalFileName = "test.txt";
            var request = new FileConvertRequest
            {
                OriginalFileName = originalFileName,
                FileContent = Encoding.UTF8.GetBytes(textContent),
                TargetFormat = "docx",
                IsPremiumUser = false
            };

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.ConvertedFileContent);
            Assert.True(result.ConvertedFileContent.Length > 0);
            Assert.Equal("test.docx", result.ConvertedFileName);
            Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.ContentType);

            using (MemoryStream ms = new MemoryStream(result.ConvertedFileContent))
            {
                using (DocX doc = DocX.Load(ms))
                {
                    Assert.Contains(textContent, doc.Text);
                }
            }
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldReturnErrorForUnsupportedTargetFormat()
        {
            var request = new FileConvertRequest
            {
                OriginalFileName = "test.txt",
                FileContent = Encoding.UTF8.GetBytes("some text"),
                TargetFormat = "unsupported",
                IsPremiumUser = false
            };

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.False(result.IsSuccess);
            Assert.Equal("Целевият формат 'unsupported' не се поддържа.", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Unsupported target format: 'unsupported'.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldReturnErrorWhenOriginalFileContentExtractionFails()
        {
            var request = new FileConvertRequest
            {
                OriginalFileName = "invalid.xyz",
                FileContent = Encoding.UTF8.GetBytes("some text"),
                TargetFormat = "pdf",
                IsPremiumUser = false
            };

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.False(result.IsSuccess);
            Assert.Equal("Извличането на съдържание от '.xyz' файлове не е имплементирано.", result.ErrorMessage);
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldReturnErrorWhenConvertedContentIsNull()
        {
            var request = new FileConvertRequest
            {
                OriginalFileName = "test.txt",
                FileContent = Encoding.UTF8.GetBytes("some text"),
                TargetFormat = "jpg",
                IsPremiumUser = false
            };

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.False(result.IsSuccess);
            Assert.Equal("Достъпът до конвертиране към 'JPG' е само за Бизнес Потребители или Администратори. Моля, надстройте абонамента си.", result.ErrorMessage);
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldConvertDocxToTxt()
        {
            var docxContent = "This is a DOCX test.";
            var docxBytes = GenerateMockDocx(docxContent);
            var originalFileName = "test.docx";

            var request = new FileConvertRequest
            {
                OriginalFileName = originalFileName,
                FileContent = docxBytes,
                TargetFormat = "txt",
                IsPremiumUser = false
            };

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.ConvertedFileContent);
            Assert.True(result.ConvertedFileContent.Length > 0);
            Assert.Equal("test.txt", result.ConvertedFileName);
            Assert.Equal("text/plain", result.ContentType);
            Assert.Contains(docxContent.Trim(), Encoding.UTF8.GetString(result.ConvertedFileContent).Trim());
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldConvertTxtToPdf()
        {
            var textContent = "This is a TXT test.";
            var originalFileName = "test.txt";
            var request = new FileConvertRequest
            {
                OriginalFileName = originalFileName,
                FileContent = Encoding.UTF8.GetBytes(textContent),
                TargetFormat = "pdf",
                IsPremiumUser = false
            };

            _mockPdfConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>()))
                             .Returns(Encoding.UTF8.GetBytes("Mock PDF content for TXT conversion"));

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.ConvertedFileContent);
            Assert.True(result.ConvertedFileContent.Length > 0);
            Assert.Equal("test.pdf", result.ConvertedFileName);
            Assert.Equal("application/pdf", result.ContentType);
            _mockPdfConverter.Verify(c => c.Convert(It.IsAny<HtmlToPdfDocument>()), Times.Once);
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldConvertXlsxToPdf()
        {
            var xlsxContent = "Col1\tCol2\nVal1\tVal2";
            var xlsxBytes = GenerateMockXlsx(xlsxContent);
            var originalFileName = "test.xlsx";
            var request = new FileConvertRequest
            {
                OriginalFileName = originalFileName,
                FileContent = xlsxBytes,
                TargetFormat = "pdf",
                IsPremiumUser = false
            };

            _mockPdfConverter.Setup(c => c.Convert(It.IsAny<HtmlToPdfDocument>()))
                             .Returns(Encoding.UTF8.GetBytes("Mock PDF content for XLSX conversion"));

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.ConvertedFileContent);
            Assert.True(result.ConvertedFileContent.Length > 0);
            Assert.Equal("test.pdf", result.ConvertedFileName);
            Assert.Equal("application/pdf", result.ContentType);
            _mockPdfConverter.Verify(c => c.Convert(It.IsAny<HtmlToPdfDocument>()), Times.Once);
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldConvertPngToJpg_PremiumUser()
        {
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var originalFileName = "image.png";
            var request = new FileConvertRequest
            {
                OriginalFileName = originalFileName,
                FileContent = imageBytes,
                TargetFormat = "jpg",
                IsPremiumUser = true
            };

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.ConvertedFileContent);
            Assert.True(result.ConvertedFileContent.Length > 0);
            Assert.Equal("image.jpg", result.ConvertedFileName);
            Assert.Equal("image/jpeg", result.ContentType);
            Assert.Equal(imageBytes, result.ConvertedFileContent);
        }

        [Fact]
        public async Task ConvertFileSpecificAsync_ShouldForbidPremiumFormat_NonPremiumUser()
        {
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var originalFileName = "image.png";
            var request = new FileConvertRequest
            {
                OriginalFileName = originalFileName,
                FileContent = imageBytes,
                TargetFormat = "jpg",
                IsPremiumUser = false
            };

            var result = await _fileConverterService.ConvertFileSpecificAsync(request, request.IsPremiumUser);

            Assert.False(result.IsSuccess);
            Assert.Equal("Достъпът до конвертиране към 'JPG' е само за Бизнес Потребители или Администратори. Моля, надстройте абонамента си.", result.ErrorMessage);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Access denied for target format 'jpg'. User is not premium (isPremiumUser was False).")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        private byte[] GenerateMockDocx(string content)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (DocX doc = DocX.Create(ms))
                {
                    doc.InsertParagraph(content);
                    doc.Save();
                }
                return ms.ToArray();
            }
        }

        private byte[] GenerateMockXlsx(string content)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    string[] lines = content.Split(new[] { '\n' }, StringSplitOptions.None);
                    for (int row = 0; row < lines.Length; row++)
                    {
                        string[] cells = lines[row].Split(new[] { '\t' }, StringSplitOptions.None);
                        for (int col = 0; col < cells.Length; col++)
                        {
                            worksheet.Cells[row + 1, col + 1].Value = cells[col].Trim();
                        }
                    }
                    package.Save();
                }
                return ms.ToArray();
            }
        }

        private byte[] GenerateMockPdf(string content)
        {
            return Encoding.UTF8.GetBytes($"%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj 2 0 obj<</Type/Pages/Count 1/Kids[3 0 R]>>endobj 3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]/Contents 4 0 R/Resources<<>>>>endobj 4 0 obj<</Length {content.Length + 10}>>stream\nBT /F1 12 Tf 100 700 Td ({content}) Tj ET\nendstream\nendobj\nxref\n0 5\n0000000000 65535 f\n0000000009 00000 n\n0000000074 00000 n\n0000000155 00000 n\n0000000252 00000 n\ntrailer<</Size 5/Root 1 0 R>>startxref\n{content.Length + 100}\n%%EOF");
        }
    }
}
