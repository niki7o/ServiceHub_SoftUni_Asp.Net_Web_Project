using DinkToPdf;
using DinkToPdf.Contracts;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ServiceHub.Common;
using ServiceHub.Core.Models.Service.FileConverter;
using ServiceHub.Services.Interfaces;
using System.Text;
using UglyToad.PdfPig;
using Xceed.Words.NET;



namespace ServiceHub.Services.Services
{
  public class FileConverterService : IFileConverterService
    {
        private readonly ILogger<FileConverterService> _logger;
        private readonly IConverter _pdfConverter;

        
        private static readonly HashSet<string> PremiumFormats = new HashSet<string> { "jpg", "png", "csv" };

        public FileConverterService(ILogger<FileConverterService> logger, IConverter pdfConverter)
        {
            _logger = logger;
            _pdfConverter = pdfConverter;
        }

        public async Task<BaseServiceResponse> ExecuteAsync(BaseServiceRequest request)
        {
            if (request is not FileConvertRequest fileConvertRequest)
            {
                _logger.LogError("Invalid request type for FileConverterService. Expected FileConvertRequest.");
                return new FileConvertResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Невалиден тип заявка за услугата за конвертиране на файлове."
                };
            }

            _logger.LogInformation($"Executing File Converter for '{fileConvertRequest.OriginalFileName}' to '{fileConvertRequest.TargetFormat}'...");

            
            var result = await ConvertFileSpecificAsync(fileConvertRequest, fileConvertRequest.IsPremiumUser);
            return result;
        }

        
        public async Task<FileConvertResult> ConvertFileSpecificAsync(FileConvertRequest request, bool isPremiumUser)
        {
            _logger.LogInformation($"FileConverterService.ConvertFileSpecificAsync: TargetFormat: {request.TargetFormat}, IsPremiumUser: {isPremiumUser}");

        
            if (PremiumFormats.Contains(request.TargetFormat.ToLowerInvariant()) && !isPremiumUser)
            {
                _logger.LogWarning($"Access denied for target format '{request.TargetFormat}'. User is not premium (isPremiumUser was {isPremiumUser}).");
                return new FileConvertResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Достъпът до конвертиране към '{request.TargetFormat.ToUpperInvariant()}' е само за Бизнес Потребители или Администратори. Моля, надстройте абонамента си."
                };
            }

            byte[]? convertedContent = null;
            string newFileName = $"{Path.GetFileNameWithoutExtension(request.OriginalFileName)}.{request.TargetFormat.ToLowerInvariant()}";
            string contentType = GetContentType(request.TargetFormat);

            FileContentData originalFileParsedContent = ExtractContentFromOriginalFile(request.FileContent, request.OriginalFileName);

            if (!originalFileParsedContent.IsSuccess)
            {
                _logger.LogError($"Failed to extract content from original file: {originalFileParsedContent.ErrorMessage}");
                return new FileConvertResult { IsSuccess = false, ErrorMessage = originalFileParsedContent.ErrorMessage };
            }

            switch (request.TargetFormat.ToLowerInvariant())
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
                case "txt":
                    convertedContent = Encoding.UTF8.GetBytes(originalFileParsedContent.TextContent ?? string.Empty);
                    contentType = "text/plain";
                    break;
                case "csv":
                    convertedContent = Encoding.UTF8.GetBytes(originalFileParsedContent.TextContent ?? string.Empty);
                    contentType = "text/csv";
                    break;
                case "jpg":
                case "png":
                    if (originalFileParsedContent.ImageBytes != null && originalFileParsedContent.ImageBytes.Length > 0)
                    {
                        convertedContent = originalFileParsedContent.ImageBytes;
                        contentType = GetContentType(request.TargetFormat);
                    }
                    else
                    {
                        _logger.LogWarning($"Cannot convert non-image content to {request.TargetFormat}.");
                        return new FileConvertResult { IsSuccess = false, ErrorMessage = $"Не може да се конвертира не-изображение към '{request.TargetFormat.ToUpperInvariant()}'." };
                    }
                    break;
                default:
                    _logger.LogWarning($"Unsupported target format: '{request.TargetFormat}'.");
                    return new FileConvertResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Целевият формат '{request.TargetFormat}' не се поддържа."
                    };
            }

            if (convertedContent == null || convertedContent.Length == 0)
            {
                _logger.LogError($"Failed to generate content for target format: {request.TargetFormat}. Converted content is null or empty.");
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
            _logger.LogInformation($"ExtractContentFromOriginalFile: Обработва се файл: '{fileName}'");

            if (fileContent == null || fileContent.Length == 0)
            {
                _logger.LogError("ExtractContentFromOriginalFile: Оригиналният файл е празен или липсва.");
                return new FileContentData { IsSuccess = false, ErrorMessage = "Оригиналният файл е празен или липсва." };
            }

            string originalFileExtension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(originalFileExtension) || originalFileExtension == ".")
            {
                _logger.LogError($"ExtractContentFromOriginalFile: Невалидно или липсващо разширение на файла: '{fileName}'. Получено разширение: '{originalFileExtension}'");
                return new FileContentData { IsSuccess = false, ErrorMessage = $"Не може да се извлече съдържание от файл без разширение или с невалидно разширение (получено: '{originalFileExtension}')." };
            }

            originalFileExtension = originalFileExtension.TrimStart('.');

            string textContent = string.Empty;
            byte[]? imageBytes = null;

            switch (originalFileExtension)
            {
                case "txt":
                case "csv":
                    try
                    {
                        textContent = Encoding.UTF8.GetString(fileContent);
                        _logger.LogInformation($"Successfully extracted text from .{originalFileExtension} file using UTF-8.");
                    }
                    catch (DecoderFallbackException)
                    {
                        try
                        {
                            Encoding win1251 = Encoding.GetEncoding("windows-1251");
                            textContent = win1251.GetString(fileContent);
                            _logger.LogInformation($"Successfully extracted text from .{originalFileExtension} file using Windows-1251.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error reading original .{originalFileExtension} file content with alternative encodings.");
                            return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при четене на текст от .{originalFileExtension} с различни кодирания." };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error reading original .{originalFileExtension} file content.");
                        return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при четене на текст от .{originalFileExtension}." };
                    }
                    break;

                case "docx":
                    try
                    {
                        using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(new MemoryStream(fileContent), false))
                        {
                            textContent = wordDocument.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
                        }
                        _logger.LogInformation("Successfully extracted text from DOCX using Open XML SDK.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error extracting text from DOCX file: {fileName}");
                        return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при извличане на текст от DOCX файл." };
                    }
                    break;

                case "pdf":
                    try
                    {
                        using (var document = PdfDocument.Open(fileContent))
                        {
                            StringBuilder pdfText = new StringBuilder();
                            foreach (var page in document.GetPages())
                            {
                                pdfText.Append(page.Text);
                            }
                            textContent = pdfText.ToString();
                        }
                        _logger.LogInformation($"Successfully extracted text from PDF using PdfPig: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error extracting text from PDF file with PdfPig: {fileName}");
                        return new FileContentData { IsSuccess = false, ErrorMessage = $"Грешка при извличане на текст от PDF файл (PdfPig)." };
                    }
                    break;

                case "jpg":
                case "png":
                case "jpeg":
                    imageBytes = fileContent;
                    textContent = $"--- Изображение '{fileName}' (OCR изисква специализирана библиотека) ---";
                    _logger.LogWarning($"Image content from .{originalFileExtension} used. OCR not implemented. Placeholder text used.");
                    break;

                case "xlsx":
                    try
                    {
                        using (var package = new ExcelPackage(new MemoryStream(fileContent)))
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
                                            var cellValue = worksheet.Cells[row, col].Text;
                                            if (!string.IsNullOrEmpty(cellValue))
                                            {
                                                stringBuilder.Append(cellValue).Append("\t");
                                            }
                                        }
                                        stringBuilder.AppendLine();
                                    }
                                    textContent = stringBuilder.ToString().Trim();
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
                    _logger.LogWarning($"Content extraction not implemented for original file type: .{originalFileExtension}.");
                    return new FileContentData { IsSuccess = false, ErrorMessage = $"Извличането на съдържание от '.{originalFileExtension}' файлове не е имплементирано." };
            }

            return new FileContentData { IsSuccess = true, TextContent = textContent, ImageBytes = imageBytes };
        }

        private byte[] GenerateDocxFromContent(string textContent, byte[]? imageBytes, string originalFileName, bool performOCR)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (DocX document = DocX.Create(stream))
                {
                    document.InsertParagraph(textContent);

                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        _logger.LogWarning("Adding images to DOCX directly from byte array is complex with DocX. Skipping image insertion for simplicity.");
                        document.InsertParagraph("--- Изображение (не е вградено, вижте логовете за подробности) ---");
                    }

                    document.Save();
                }
                return stream.ToArray();
            }
        }

        private byte[] GeneratePdfFromContent(string textContent, byte[]? imageBytes, string originalFileName)
        {
            try
            {
                StringBuilder htmlBuilder = new StringBuilder();
                htmlBuilder.Append(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Конвертиран файл</title>
                    <style>
                        body { font-family: 'Inter', sans-serif; line-height: 1.6; color: #333; margin: 20px; }
                        .content-section { margin-bottom: 20px; }
                        img { max-width: 100%; height: auto; display: block; margin: 10px 0; }
                    </style>
                </head>
                <body>
                    <div class='content-section'>");

                if (!string.IsNullOrEmpty(textContent))
                {
                    htmlBuilder.Append($"<p>{textContent.Replace(Environment.NewLine, "<br/>")}</p>");
                }
                else
                {
                    htmlBuilder.Append("<p>Няма наличен текст за конвертиране.</p>");
                }

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    string base64Image = Convert.ToBase64String(imageBytes);
                    string imageMimeType = GetContentType(Path.GetExtension(originalFileName)?.TrimStart('.') ?? "png");
                    htmlBuilder.Append($"<img src='data:{imageMimeType};base64,{base64Image}' alt='Изображение' />");
                }

                htmlBuilder.Append(@"
                    </div>
                </body>
                </html>");

                string htmlContent = htmlBuilder.ToString();
                _logger.LogInformation($"Generated HTML for PDF (DinkToPdf): {htmlContent.Substring(0, Math.Min(htmlContent.Length, 500))}...");

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings() { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                    },
                    Objects = {
                        new ObjectSettings() {
                            HtmlContent = htmlContent,
                            WebSettings = { DefaultEncoding = "utf-8" }
                        }
                    }
                };

                byte[] pdf = _pdfConverter.Convert(doc);
                _logger.LogInformation($"PDF generation result (DinkToPdf): {(pdf != null && pdf.Length > 0 ? "Success" : "Failed")}. Length: {pdf?.Length ?? 0}");
                return pdf;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating PDF from content for '{originalFileName}' using DinkToPdf.");
                return Encoding.UTF8.GetBytes($"Грешка при генериране на PDF (DinkToPdf): {ex.Message}");
            }
        }

        private byte[] GenerateXlsxFromContent(string textContent, string originalFileName)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ExcelPackage package = new ExcelPackage(ms))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        if (!string.IsNullOrEmpty(textContent))
                        {
                            string[] lines = textContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            for (int row = 0; row < lines.Length; row++)
                            {
                                string[] cells = lines[row].Split(new[] { '\t', ',' }, StringSplitOptions.None);
                                for (int col = 0; col < cells.Length; col++)
                                {
                                    worksheet.Cells[row + 1, col + 1].Value = cells[col].Trim();
                                }
                            }
                        }
                        else
                        {
                            worksheet.Cells["A1"].Value = "Няма наличен текст за конвертиране.";
                        }

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        if (!string.IsNullOrEmpty(textContent) && textContent.Contains(Environment.NewLine))
                        {
                            worksheet.Cells["A1:" + worksheet.Dimension.End.Address.Substring(0, 1) + "1"].Style.Font.Bold = true;
                            worksheet.Cells["A1:" + worksheet.Dimension.End.Address.Substring(0, 1) + "1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells["A1:" + worksheet.Dimension.End.Address.Substring(0, 1) + "1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }

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
            return targetFormat.ToLowerInvariant() switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "txt" => "text/plain",
                "csv" => "text/csv",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        private class FileContentData
        {
            public bool IsSuccess { get; set; } = true;
            public string? ErrorMessage { get; set; }
            public string TextContent { get; set; } = string.Empty;
            public byte[]? ImageBytes { get; set; }
        }
    }

}