using iText.Html2pdf;
using iText.Html2pdf.Exceptions;
using Microsoft.Extensions.Logging;
using ServiceHub.Common;
using ServiceHub.Core.Models;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using DinkToPdf;
using DinkToPdf.Contracts;


namespace ServiceHub.Services.Services
{
    public class CvGeneratorService : ICvGeneratorService
    {
        private readonly ILogger<CvGeneratorService> _logger;
        private readonly IConverter _converter; 

        public CvGeneratorService(ILogger<CvGeneratorService> logger, IConverter converter)
        {
            _logger = logger;
            _converter = converter;
        }

        public async Task<CvGenerateResult> GenerateCvAsync(CvGenerateRequestModel request)
        {
            _logger.LogInformation("Attempting to generate CV for {Name}.", request.Name);

            try
            {
                string htmlContent = GenerateCvHtml(request);
                _logger.LogDebug("Generated HTML Content (first 500 chars): {HtmlSnippet}", htmlContent.Substring(0, Math.Min(htmlContent.Length, 500)));

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

                byte[] pdf = _converter.Convert(doc);

                _logger.LogInformation("CV successfully generated as PDF for {Name}.", request.Name);
                return new CvGenerateResult
                {
                    IsSuccess = true,
                    GeneratedFileContent = pdf,
                    GeneratedFileName = $"cv_{request.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf",
                    ContentType = "application/pdf"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error occurred during CV generation for {Name}: {Message}", request.Name, ex.Message);
                return new CvGenerateResult { IsSuccess = false, ErrorMessage = $"Възникна грешка при генериране на CV: {ex.Message}" };
            }
        }

        private string GenerateCvHtml(CvGenerateRequestModel request)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.Append($@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>CV - {request.Name}</title>
                <style>
                    body {{ font-family: 'Inter', sans-serif; line-height: 1.6; color: #333; margin: 20px; }}
                    .container {{ max-width: 800px; margin: auto; padding: 20px; border: 1px solid #eee; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                    h1 {{ color: #0056b3; text-align: center; margin-bottom: 20px; }}
                    h2 {{ color: #0056b3; border-bottom: 2px solid #eee; padding-bottom: 5px; margin-top: 20px; margin-bottom: 10px; }}
                    p {{ margin-bottom: 5px; }}
                    ul {{ list-style-type: none; padding: 0; }}
                    ul li {{ margin-bottom: 5px; }}
                    .contact-info {{ text-align: center; margin-bottom: 20px; }}
                    .section {{ margin-bottom: 15px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h1>{request.Name}</h1>
                    <div class='contact-info'>
                        <p>{request.Email}</p>
                        {(string.IsNullOrEmpty(request.Phone) ? "" : $"<p>{request.Phone}</p>")}
                    </div>

                    <div class='section'>
                        <h2>Образование</h2>
                        <p>{request.Education}</p>
                    </div>

                    <div class='section'>
                        <h2>Професионален Опит</h2>
                        <p>{request.Experience}</p>
                    </div>

                    <div class='section'>
                        <h2>Умения</h2>
                        <p>{request.Skills}</p>
                    </div>
                </div>
            </body>
            </html>
            ");

            return htmlBuilder.ToString();
        }
    }
}

