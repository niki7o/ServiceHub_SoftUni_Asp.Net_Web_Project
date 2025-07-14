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
                case "txt":
                    convertedContent = GenerateTxtFromContent(originalFileParsedContent.TextContent);
                    break;
                case "csv":
                    convertedContent = GenerateCsvFromContent(originalFileParsedContent.TextContent);
                    break;
                case "pdf":
                    convertedContent = GeneratePdfFromContent(originalFileParsedContent.TextContent, originalFileParsedContent.ImageBytes, request.OriginalFileName);
                    break;
                case "jpg":
                    convertedContent = GenerateJpgFromContent(originalFileParsedContent.TextContent, originalFileParsedContent.ImageBytes, request.OriginalFileName);
                    break;
                case "png":
                    convertedContent = GeneratePngFromContent(originalFileParsedContent.TextContent, originalFileParsedContent.ImageBytes, request.OriginalFileName);
                    break;
                case "xlsx":
                    convertedContent = GenerateXlsxFromContent(originalFileParsedContent.TextContent, request.OriginalFileName);
                    break;
                default:
                    _logger.LogWarning($"Unsupported target format: '{request.TargetFormat}'.");
                    return new FileConvertResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Целевият формат '{request.TargetFormat}' не се поддържа или не е имплементиран."
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
                ContentType = contentType
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

                        using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(new MemoryStream(fileContent), false))
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
                       
                        using (var pdfReader = new iText.Kernel.Pdf.PdfReader(new MemoryStream(fileContent)))
                        using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                        {
                            for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                            {
                                textContent += iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum));
                            }
                        }
                        textContent = $"--- Текст от PDF '{fileName}' (извличане изисква PDF библиотека) ---";
                        _logger.LogWarning($"Text extraction from PDF not fully implemented. Placeholder used.");
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
                    textContent = $"--- Изображение '{fileName}' (OCR изисква специализирана библиотека) ---";
                    _logger.LogWarning($"Image content from {originalFileExtension} used. OCR not implemented. Placeholder text used.");
                    break;

                case ".xlsx":
                    try
                    {
                       
                        using (var package = new ExcelPackage(new MemoryStream(fileContent)))
                        {
                            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                            if (worksheet != null)
                            {
                               
                                 textContent = string.Join(Environment.NewLine,
                                    worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column]
                                    .Select(cell => cell.Value?.ToString() ?? "").ToList());
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
                using (DocX document = DocX.Create(stream))
                {
                    document.InsertParagraph(textContent);
                    document.Save();
                }
                return stream.ToArray();
            }
        }

        private byte[] GenerateTxtFromContent(string textContent)
        {
            return Encoding.UTF8.GetBytes(textContent);
        }

        private byte[] GenerateCsvFromContent(string textContent)
        {
            return Encoding.UTF8.GetBytes(textContent);
        }

        private byte[] GeneratePdfFromContent(string textContent, byte[] imageBytes, string originalFileName)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Използваме пълните имена на класовете, за да избегнем конфликти на имена
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

                    // Ако имате изображения, можете да ги добавите така (изисква iText.IO.Image)
                    // if (imageBytes != null && imageBytes.Length > 0)
                    // {
                    //     iText.IO.Image.ImageData imageData = iText.IO.Image.ImageDataFactory.Create(imageBytes);
                    //     iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData);
                    //     document.Add(image);
                    // }

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

        private byte[] GenerateJpgFromContent(string textContent, byte[] imageBytes, string originalFileName)
        {
            _logger.LogWarning("JPG generation not fully implemented. Placeholder used.");
            return Encoding.UTF8.GetBytes($"--- JPG генериране от '{originalFileName}' (изисква Image библиотека) ---\n" +
                                           $"Текст: {textContent}\n" +
                                           $"Изображение: {(imageBytes != null ? "Налично" : "Неналично")}");
        }

        private byte[] GeneratePngFromContent(string textContent, byte[] imageBytes, string originalFileName)
        {
            _logger.LogWarning("PNG generation not fully implemented. Placeholder used.");
            return Encoding.UTF8.GetBytes($"--- PNG генериране от '{originalFileName}' (изисква Image библиотека) ---\n" +
                                           $"Текст: {textContent}\n" +
                                           $"Изображение: {(imageBytes != null ? "Налично" : "Неналично")}");
        }

        private byte[] GenerateXlsxFromContent(string textContent, string originalFileName)
        {
            try
            {
                // Задайте лицензния контекст за EPPlus, ако е необходимо (веднъж при стартиране на приложението)
                // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (ExcelPackage package = new ExcelPackage(ms))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        if (!string.IsNullOrEmpty(textContent))
                        {
                            // Пример за записване на текст в клетка A1
                            worksheet.Cells["A1"].Value = textContent;

                            // Ако textContent е във формат CSV, можете да го заредите директно:
                            // worksheet.Cells["A1"].LoadFromText(textContent, new ExcelTextFormat { Delimiter = ',' });

                            // За форматиране:
                            worksheet.Cells["A1"].Style.Font.Bold = true;
                            worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                        else
                        {
                            worksheet.Cells["A1"].Value = "Няма наличен текст за конвертиране.";
                        }

                        // Автоматично нагласяне на ширината на колоната
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
                "jpg" => "image/jpeg",
                "png" => "image/png",
                "txt" => "text/plain",
                "csv" => "text/csv",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
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
