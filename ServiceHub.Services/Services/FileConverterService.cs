using Microsoft.Extensions.Logging;
using ServiceHub.Common;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Services.Interfaces;
using System.Text;
using Xceed.Words.NET; 
using OfficeOpenXml; 
using OfficeOpenXml.Style; 
using DocumentFormat.OpenXml.Packaging;



namespace ServiceHub.Services.Services
{
    public class FileConverterService : IFileConverterService
    {
        private readonly ILogger<FileConverterService> _logger;

        public FileConverterService(ILogger<FileConverterService> logger)
        {
            _logger = logger;
        }

        public async Task<BaseServiceResponse> ExecuteAsync(BaseServiceRequest request)
        {
            if (request is not FileConvertRequest fileConvertRequest)
            {
                _logger.LogError("Invalid request type for FileConverterService.");
                return new FileConvertResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Невалиден тип заявка за услугата за конвертиране на файлове."
                };
            }

            _logger.LogInformation($"Executing File Converter for '{fileConvertRequest.OriginalFileName}' to '{fileConvertRequest.TargetFormat}'...");

            var result = await ConvertFileSpecificAsync(fileConvertRequest);
            return result;
        }

        public async Task<FileConvertResult> ConvertFileSpecificAsync(FileConvertRequest request)
        {
            byte[] convertedContent = null;
            // НЕ ПРОМЕНЯМЕ ИМЕТО НА ФАЙЛА, ИЗПОЛЗВАМЕ ОРИГИНАЛНОТО ИМЕ С НОВИЯ ФОРМАТ
            string newFileName = $"{Path.GetFileNameWithoutExtension(request.OriginalFileName)}.{request.TargetFormat}";
            string contentType = GetContentType(request.TargetFormat);

            FileContentData originalFileParsedContent = ExtractContentFromOriginalFile(request.FileContent, request.OriginalFileName);

            if (!originalFileParsedContent.IsSuccess)
            {
                _logger.LogError($"Failed to extract content from original file: {originalFileParsedContent.ErrorMessage}");
                return new FileConvertResult { IsSuccess = false, ErrorMessage = originalFileParsedContent.ErrorMessage };
            }

            switch (request.TargetFormat.ToLower())
            {
                case "docx":
                    convertedContent = GenerateDocxFromContent(originalFileParsedContent.TextContent, originalFileParsedContent.ImageBytes, request.OriginalFileName, request.PerformOCRIfApplicable);
                    break;
                case "pdf":
                    convertedContent = GeneratePdfFromContent(originalFileParsedContent.TextContent, originalFileParsedContent.ImageBytes, request.OriginalFileName);
                    break;
                case "xlsx":
                    convertedContent = GenerateXlsxFromContent(originalFileParsedContent.TextContent, request.OriginalFileName);
                    break;
                default:
                    // Only DOCX, PDF, XLSX are now supported
                    _logger.LogWarning($"Unsupported target format: '{request.TargetFormat}'. Only DOCX, PDF, XLSX are supported.");
                    return new FileConvertResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Целевият формат '{request.TargetFormat}' не се поддържа. Моля, изберете DOCX, PDF или XLSX."
                    };
            }

            if (convertedContent == null || convertedContent.Length == 0)
            {
                _logger.LogError($"Failed to generate content for target format: {request.TargetFormat}.");
                return new FileConvertResult { IsSuccess = false, ErrorMessage = $"Неуспешно генериране на съдържание за '{request.TargetFormat}'." };
            }

            return new FileConvertResult
            {
                IsSuccess = true,
                ConvertedFileContent = convertedContent,
                ConvertedFileName = newFileName, 
                ContentType = contentType,
                OriginalFileName = request.OriginalFileName, 
                TargetFormat = request.TargetFormat 
            };
        }

        private FileContentData ExtractContentFromOriginalFile(byte[] fileContent, string fileName)
        {
            string originalFileExtension = Path.GetExtension(fileName)?.ToLower();
            string textContent = string.Empty;
            byte[] imageBytes = null;

            if (fileContent == null || fileContent.Length == 0)
            {
                return new FileContentData { IsSuccess = false, ErrorMessage = "Оригиналният файл е празен или липсва." };
            }

            switch (originalFileExtension)
            {
                case ".txt":
                case ".csv":
                    try
                    {
                        textContent = Encoding.UTF8.GetString(fileContent);
                        _logger.LogInformation($"Successfully extracted text from {originalFileExtension} file using UTF-8.");
                    }
                    catch (DecoderFallbackException)
                    {
                        try
                        {
                            Encoding win1251 = Encoding.GetEncoding("windows-1251");
                            textContent = win1251.GetString(fileContent);
                            _logger.LogInformation($"Successfully extracted text from {originalFileExtension} file using Windows-1251.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error reading original {originalFileExtension} file content with alternative encodings.");
                            return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при четене на текст от {originalFileExtension} с различни кодирания." };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error reading original {originalFileExtension} file content.");
                        return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при четене на текст от {originalFileExtension}." };
                    }
                    break;

                case ".docx":
                    try
                    {
                        using (DocumentFormat.OpenXml.Packaging.WordprocessingDocument wordDocument = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(new MemoryStream(fileContent), false))
                        {
                            textContent = wordDocument.MainDocumentPart.Document.Body.InnerText;
                        }
                        _logger.LogInformation("Successfully extracted text from DOCX using Open XML SDK.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error extracting text from DOCX file: {fileName}");
                        return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при извличане на текст от DOCX файл." };
                    }
                    break;

                case ".pdf":
                    try
                    {
                        // Извличане на текст от PDF с iText7
                        using (var pdfReader = new iText.Kernel.Pdf.PdfReader(new MemoryStream(fileContent)))
                        using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                        {
                            StringBuilder pdfText = new StringBuilder();
                            for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                            {
                                pdfText.Append(iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum)));
                            }
                            textContent = pdfText.ToString();
                        }
                        _logger.LogInformation($"Successfully extracted text from PDF: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error extracting text from PDF file: {fileName}");
                        return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при извличане на текст от PDF файл." };
                    }
                    break;

                case ".jpg":
                case ".png":
                    imageBytes = fileContent;
                    textContent = $"--- Изображение '{fileName}' (OCR изисква специализирана библиотека) ---"; // OCR не е имплементирано
                    _logger.LogWarning($"Image content from {originalFileExtension} used. OCR not implemented. Placeholder text used.");
                    break;

                case ".xlsx":
                    try
                    {
                        using (var package = new OfficeOpenXml.ExcelPackage(new MemoryStream(fileContent)))
                        {
                            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                            if (worksheet != null)
                            {
                                var stringBuilder = new StringBuilder();
                                var dimension = worksheet.Dimension;
                                if (dimension != null)
                                {
                                    for (int row = dimension.Start.Row; row <= dimension.End.Row; row++)
                                    {
                                        for (int col = dimension.Start.Column; col <= dimension.End.Column; col++)
                                        {
                                            var cellValue = worksheet.Cells[row, col].Text; // Използваме .Text за форматирана стойност
                                            if (!string.IsNullOrEmpty(cellValue))
                                            {
                                                stringBuilder.Append(cellValue).Append("\t"); // Разделител с таб
                                            }
                                        }
                                        stringBuilder.AppendLine(); // Нов ред за всеки ред в Excel
                                    }
                                    textContent = stringBuilder.ToString().Trim(); // Премахваме излишните празни места
                                }
                                else
                                {
                                    textContent = "Празен работен лист в XLSX файл.";
                                }
                            }
                            else
                            {
                                textContent = "Не е намерен работен лист в XLSX файл.";
                            }
                        }
                        _logger.LogInformation("Successfully extracted data from XLSX using EPPlus.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error extracting data from XLSX file: {fileName}");
                        return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при извличане на данни от XLSX файл." };
                    }
                    break;

                default:
                    _logger.LogWarning($"Content extraction not implemented for original file type: {originalFileExtension}.");
                    return new FileContentData { IsSuccess = false, ErrorMessage = $"Извличането на съдържание от '{originalFileExtension}' файлове не е имплементирано." };
            }

            return new FileContentData { IsSuccess = true, TextContent = textContent, ImageBytes = imageBytes };
        }

        private byte[] GenerateDocxFromContent(string textContent, byte[] imageBytes, string originalFileName, bool performOCR)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (Xceed.Words.NET.DocX document = Xceed.Words.NET.DocX.Create(stream))
                {
                    document.InsertParagraph(textContent);
                    document.Save();
                }
                return stream.ToArray();
            }
        }

        private byte[] GeneratePdfFromContent(string textContent, byte[] imageBytes, string originalFileName)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(ms);
                    iText.Kernel.Pdf.PdfDocument pdf = new iText.Kernel.Pdf.PdfDocument(writer);
                    iText.Layout.Document document = new iText.Layout.Document(pdf);

                    document.Add(new iText.Layout.Element.Paragraph("Конвертирано съдържание:"));
                    if (!string.IsNullOrEmpty(textContent))
                    {
                        document.Add(new iText.Layout.Element.Paragraph(textContent));
                    }
                    else
                    {
                        document.Add(new iText.Layout.Element.Paragraph("Няма наличен текст за конвертиране."));
                    }

                    // Ако имате изображения, можете да ги добавите така
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        try
                        {
                            iText.IO.Image.ImageData imageData = iText.IO.Image.ImageDataFactory.Create(imageBytes);
                            iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData);
                            document.Add(image);
                        }
                        catch (Exception imgEx)
                        {
                            _logger.LogError(imgEx, "Failed to add image to PDF.");
                            document.Add(new iText.Layout.Element.Paragraph("Грешка при добавяне на изображение към PDF."));
                        }
                    }

                    document.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating PDF from content for '{originalFileName}'.");
                return Encoding.UTF8.GetBytes($"Грешка при генериране на PDF: {ex.Message}");
            }
        }

        private byte[] GenerateXlsxFromContent(string textContent, string originalFileName)
        {
            try
            {
                // Задайте лицензния контекст за EPPlus, ако е необходимо (веднъж при стартиране на приложението)
                // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(ms))
                    {
                        OfficeOpenXml.ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        if (!string.IsNullOrEmpty(textContent))
                        {
                            // Разделяме текста на редове и колони (ако е CSV-подобен)
                            string[] lines = textContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            for (int row = 0; row < lines.Length; row++)
                            {
                                string[] cells = lines[row].Split(new[] { '\t' }, StringSplitOptions.None); // Използваме таб като разделител
                                for (int col = 0; col < cells.Length; col++)
                                {
                                    worksheet.Cells[row + 1, col + 1].Value = cells[col];
                                }
                            }
                        }
                        else
                        {
                            worksheet.Cells["A1"].Value = "Няма наличен текст за конвертиране.";
                        }

                        // Автоматично нагласяне на ширината на колоните
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        package.Save();
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating XLSX from content for '{originalFileName}'.");
                return Encoding.UTF8.GetBytes($"Грешка при генериране на XLSX: {ex.Message}");
            }
        }

        private string GetContentType(string targetFormat)
        {
            return targetFormat.ToLower() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream" // Default for unsupported types
            };
        }

        private class FileContentData
        {
            public bool IsSuccess { get; set; } = true;
            public string ErrorMessage { get; set; }
            public string TextContent { get; set; } = string.Empty;
            public byte[] ImageBytes { get; set; }
        }
    }

}
