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
            _logger.LogInformation($"GenerateCvAsync: Започва генериране на CV за {request.Name}.");

            string htmlContent = GenerateCvHtml(request);
            _logger.LogDebug($"GenerateCvAsync: Генериран HTML:\n{htmlContent}");

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
                        WebSettings = {
                            DefaultEncoding = "utf-8"
                        }
                    }
                }
            };

            byte[] pdfBytes;
            try
            {
                pdfBytes = _converter.Convert(doc);
                _logger.LogInformation($"GenerateCvAsync: PDF генериран успешно. Дължина: {pdfBytes.Length} байта.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateCvAsync: Грешка при конвертиране на HTML в PDF с DinkToPdf.");
                return new CvGenerateResult { IsSuccess = false, ErrorMessage = $"Грешка при генериране на PDF: {ex.Message}" };
            }

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                _logger.LogError("GenerateCvAsync: DinkToPdf върна празен или null PDF.");
                return new CvGenerateResult { IsSuccess = false, ErrorMessage = "Неуспешно генериране на PDF файл (празен резултат)." };
            }

            return new CvGenerateResult
            {
                IsSuccess = true,
                GeneratedFileContent = pdfBytes,
                GeneratedFileName = $"{request.Name.Replace(" ", "_")}_CV.pdf",
                ContentType = "application/pdf"
            };
        }

        private string GenerateCvHtml(CvGenerateRequestModel request)
        {
             
            StringBuilder htmlBuilder = new StringBuilder();
            string ProcessTextAreaContent(string content)
            {
                if (string.IsNullOrEmpty(content)) return "";

                return content.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
            }
            htmlBuilder.Append($@"
            <!DOCTYPE html>
            <html lang='bg'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Моето CV - {request.Name}</title>
                <!-- Премахнат Google Fonts линк, за да се разчита на системни шрифтове -->
                <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css'>
                <style>
                    /* Основни стилове */
                    body {{
                       
                        font-family: 'Arial', 'Times New Roman', 'Verdana', 'Segoe UI', 'Helvetica', sans-serif;
                        background-color: #f0f2f5;
                        display: flex;
                        justify-content: center;
                        align-items: flex-start;
                        min-height: 100vh;
                        padding: 30px;
                        box-sizing: border-box;
                    }}

                    .cv-container {{
                        background-color: #ffffff;
                        border-radius: 12px;
                        box-shadow: 0 15px 40px rgba(0, 0, 0, 0.15);
                        width: 100%;
                        max-width: 980px;
                        display: flex;
                        flex-direction: column;
                        overflow: hidden;
                    }}

                    /* Заглавна секция (Име и контакти) */
                    .header-section {{
                        background-color: #2c3e50;
                        color: #ffffff;
                        padding: 45px 60px;
                        text-align: center;
                        border-bottom: 6px solid #3498db;
                    }}

                    .header-section h1 {{
                        font-size: 4rem;
                        font-weight: 800;
                        margin-bottom: 12px;
                        letter-spacing: -0.06em;
                    }}

                    .header-section .contact-info-group {{
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        gap: 25px;
                        margin-top: 10px;
                        font-size: 1.2rem;
                        font-weight: 300;
                    }}

                    .header-section .contact-info-group p {{
                        display: flex;
                        align-items: center;
                        gap: 8px;
                    }}

                    .header-section .contact-info-group i {{
                        color: #ecf0f1;
                        font-size: 1.3rem;
                    }}

                    /* Основно съдържание (Секции) */
                    .main-content {{
                        padding: 40px 60px;
                        display: grid;
                        grid-template-columns: 1fr;
                        gap: 40px;
                    }}

                    .section {{
                        padding-bottom: 30px;
                        border-bottom: 1px solid #e0e0e0;
                    }}

                    .section:last-child {{
                        border-bottom: none;
                    }}

                    .section-title {{
                        font-size: 2.4rem;
                        font-weight: 700;
                        color: #34495e;
                        margin-bottom: 20px;
                        display: flex;
                        align-items: center;
                        gap: 15px;
                        text-transform: uppercase;
                        letter-spacing: 0.08em;
                        padding-bottom: 8px;
                        border-bottom: 3px solid #3498db;
                        width: fit-content;
                    }}

                    .section-title i {{
                        color: #3498db;
                        font-size: 2.2rem;
                    }}

                    /* Елементи на секциите */
                    .item-description {{
                        font-size: 1.15rem;
                        color: #555;
                        line-height: 1.7;
                        margin-top: 12px;
                        white-space: pre-wrap;
                    }}

                    .skill-list {{
                        display: flex;
                        flex-wrap: wrap;
                        gap: 15px;
                    }}

                    .skill-item {{
                        background-color: #e8f6fd;
                        color: #2980b9;
                        padding: 10px 20px;
                        border-radius: 25px;
                        font-size: 1.05rem;
                        font-weight: 600;
                        border: 1px solid #aed6f1;
                        transition: all 0.2s ease-in-out;
                    }}

                    .skill-item:hover {{
                        background-color: #d0efff;
                        transform: translateY(-2px);
                        box-shadow: 0 4px 8px rgba(0,0,0,0.1);
                    }}

                    /* Responsive adjustments */
                    @media (min-width: 768px) {{
                        .main-content {{
                            grid-template-columns: 2.5fr 1.5fr;
                        }}

                        .header-section h1 {{
                            font-size: 4.5rem;
                        }}

                        .header-section .contact-info-group {{
                            font-size: 1.35rem;
                        }}

                        .section-title {{
                            font-size: 2.6rem;
                        }}

                        .item-description {{
                            font-size: 1.2rem;
                        }}
                    }}

                    /* Специални стилове за печат (ако wkhtmltopdf ги поддържа) */
                    @media print {{
                        body {{
                            background-color: #ffffff;
                            margin: 0;
                            padding: 0;
                        }}
                        .cv-container {{
                            box-shadow: none;
                            border-radius: 0;
                            max-width: none;
                            width: 100%;
                        }}
                        .header-section {{
                            padding: 30px 40px;
                            border-bottom: 4px solid #3498db;
                        }}
                        .main-content {{
                            padding: 30px 40px;
                        }}
                        .section {{
                            padding-bottom: 20px;
                        }}
                        .section-title {{
                            font-size: 1.8rem;
                            border-bottom: 2px solid #3498db;
                        }}
                        .item-description, .header-section .contact-info-group p {{
                            font-size: 1rem;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class='cv-container'>
                    <!-- Заглавна секция -->
                    <div class='header-section'>
                        <h1>{request.Name}</h1>
                        <div class='contact-info-group'>
                            <p><i class='fas fa-envelope'></i> {request.Email}</p>
                            {(string.IsNullOrEmpty(request.Phone) ? "" : $"<p><i class='fas fa-phone'></i> {request.Phone}</p>")}
                        </div>
                    </div>

                    <!-- Основно съдържание -->
                    <div class='main-content'>
                        <div class='left-column'>
                            {(string.IsNullOrEmpty(request.Summary) ? "" : $@"
                            <div class='section'>
                                <h2 class='section-title'><i class='fas fa-user-circle'></i> Профил</h2>
                                <p class='item-description'>{request.Summary.Replace(Environment.NewLine, "<br/>")}</p>
                            </div>
                            ")}

                            {(string.IsNullOrEmpty(request.Experience) ? "" : $@"
                            <div class='section'>
                                <h2 class='section-title'><i class='fas fa-briefcase'></i> Професионален Опит</h2>
                                <p class='item-description'>{request.Experience.Replace(Environment.NewLine, "<br/>")}</p>
                            </div>
                            ")}
                        </div>

                        <div class='right-column'>
                            {(string.IsNullOrEmpty(request.Education) ? "" : $@"
                            <div class='section'>
                                <h2 class='section-title'><i class='fas fa-graduation-cap'></i> Образование</h2>
                                <p class='item-description'>{request.Education.Replace(Environment.NewLine, "<br/>")}</p>
                            </div>
                            ")}

                            {(string.IsNullOrEmpty(request.Skills) ? "" : $@"
                            <div class='section'>
                                <h2 class='section-title'><i class='fas fa-lightbulb'></i> Умения</h2>
                                <div class='skill-list'>
                                    {string.Join("", request.Skills.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => $"<span class='skill-item'>{s.Trim()}</span>"))}
                                </div>
                            </div>
                            ")}
                        </div>
                    </div>
                </div>
            </body>
            </html>");

            return htmlBuilder.ToString();
        }
    }
}

