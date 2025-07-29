using DinkToPdf;
using DinkToPdf.Contracts;
using iText.Html2pdf;
using iText.Html2pdf.Exceptions;
using Microsoft.Extensions.Logging;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services
{
    public class ContractGeneratorService : IContractGeneratorService
    {
        private readonly ILogger<ContractGeneratorService> _logger;
        private readonly IConverter _converter;

        public ContractGeneratorService(ILogger<ContractGeneratorService> logger, IConverter converter)
        {
            _logger = logger;
            _converter = converter;
        }

        public async Task<ContractGenerateResult> GenerateContractAsync(ContractGenerateRequestModel request)
        {
            _logger.LogInformation("Attempting to generate contract for {ContractType} between {PartyA} and {PartyB}.", request.ContractType, request.PartyA, request.PartyB);

            try
            {
                string htmlContent = GenerateContractHtml(request);
                _logger.LogDebug("Generated HTML Content (first 500 chars): {HtmlSnippet}", htmlContent.Substring(0, Math.Min(htmlContent.Length, 500)));

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings() { Top = 15, Bottom = 15, Left = 20, Right = 20 },
                    },
                    Objects = {
                        new ObjectSettings() {
                            HtmlContent = htmlContent,
                            WebSettings = { DefaultEncoding = "utf-8" }
                        }
                    }
                };

                byte[] pdf = _converter.Convert(doc);

                
                return new ContractGenerateResult
                {
                    IsSuccess = true,
                    GeneratedFileContent = pdf,
                    GeneratedFileName = $"{request.ContractType?.Replace(" ", "_")}_{request.PartyA?.Replace(" ", "_")}_{request.PartyB?.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf",
                    ContentType = "application/pdf"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error occurred during contract generation for {ContractType}: {Message}", request.ContractType, ex.Message);
                return new ContractGenerateResult { IsSuccess = false, ErrorMessage = $"Възникна грешка при генериране на договор: {ex.Message}" };
            }
        }

        private string GenerateContractHtml(ContractGenerateRequestModel request)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            
            string processedContractTerms = ConvertPlainTextToHtml(request.ContractTerms);

          
            string additionalInfoContent = request.AdditionalInfo.Replace(Environment.NewLine, "<br/>");

            htmlBuilder.Append($@"
            <!DOCTYPE html>
            <html lang='bg'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Договор за {request.ContractType ?? "Услуга"}</title>
                <style>
                    body {{
                        font-family: 'Arial', 'Times New Roman', 'Verdana', 'Segoe UI', 'Helvetica', sans-serif;
                        line-height: 1.8;
                        color: #333;
                        margin: 0;
                        padding: 0;
                        background-color: #f9f9f9;
                        font-size: 11pt;
                    }}
                    .container {{
                        max-width: 850px;
                        margin: 30px auto;
                        background: #fff;
                        padding: 40px 50px;
                        border-radius: 10px;
                        box-shadow: 0 5px 20px rgba(0,0,0,0.08);
                    }}
                    h1 {{
                        color: #004085;
                        text-align: center;
                        margin-bottom: 25px;
                        font-size: 2.5em;
                        font-weight: 700;
                        text-transform: uppercase;
                        letter-spacing: 1px;
                    }}
                    h2 {{
                        color: #0056b3;
                        border-bottom: 2px solid #007bff;
                        padding-bottom: 10px;
                        margin-top: 35px;
                        margin-bottom: 20px;
                        font-size: 1.6em;
                        font-weight: 600;
                    }}
                    h3 {{ /* Използваме H3 за подзаглавия в списъци, ако се наложи */
                        color: #0056b3;
                        font-size: 1.2em;
                        margin-bottom: 10px;
                    }}
                    p {{
                        margin-bottom: 10px;
                    }}
                    hr {{
                        border: none;
                        border-top: 1px solid #eee;
                        margin: 20px 0;
                    }}
                    ul {{
                        list-style: none; /* Премахване на стандартните точки */
                        padding-left: 0;
                    }}
                    ul li {{
                        margin-bottom: 5px;
                        padding-left: 20px; /* Отстъп за персонализираната точка */
                        position: relative;
                    }}
                    ul li::before {{
                        content: '•'; /* Персонализирана точка */
                        color: #007bff;
                        position: absolute;
                        left: 0;
                        top: 0;
                        font-weight: bold;
                    }}
                    /* Стилове за удебелен и подчертан текст */
                    strong {{
                        font-weight: bold;
                    }}
                    u {{
                        text-decoration: underline;
                        white-space: pre; /* Запазва интервалите под подчертаването */
                    }}
                    i {{
                        font-style: italic;
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 40px;
                    }}
                    .header p {{
                        font-size: 1.1em;
                        color: #555;
                    }}
                    .party-info {{ /* Този клас вече не се използва директно, но го запазвам за справка */
                        margin-bottom: 30px;
                        border: 1px solid #e0e0e0;
                        padding: 20px;
                        border-radius: 8px;
                        background-color: #f8f8f8;
                    }}
                    .party-info p {{
                        margin: 8px 0;
                    }}
                    .party-info strong {{
                        color: #333;
                    }}
                    .party-info i {{
                        color: #777;
                        font-size: 0.9em;
                    }}
                    .section {{ /* Този клас вече не се използва директно, но го запазвам за справка */
                        margin-bottom: 30px;
                    }}
                    .terms {{ /* Този клас вече не се използва директно, но го запазвам за справка */
                        white-space: pre-wrap;
                        background-color: #f0f8ff;
                        padding: 20px;
                        border-radius: 8px;
                        border: 1px dashed #aaddff;
                        line-height: 1.7;
                        font-size: 11pt;
                    }}
                    .signature-block {{
                        margin-top: 50px;
                        display: flex;
                        justify-content: space-around;
                        text-align: center;
                        flex-wrap: wrap;
                    }}
                    .signature-block > div {{
                        width: 45%;
                        margin: 15px 0;
                    }}
                    .signature-line {{
                        border-top: 1px solid #999;
                        width: 80%;
                        margin: 10px auto 5px auto;
                        padding-top: 5px;
                    }}
                    .date-info {{
                        text-align: right;
                        margin-top: 40px;
                        font-style: italic;
                        color: #666;
                        font-size: 0.95em;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    {processedContractTerms}
            ");

            if (!string.IsNullOrWhiteSpace(request.AdditionalInfo))
            {
                htmlBuilder.Append($@"
                    <h2>ДОПЪЛНИТЕЛНА ИНФОРМАЦИЯ</h2>
                    <p>{additionalInfoContent}</p>
                ");
            }

            htmlBuilder.Append($@"
                    <div class='date-info'>
                        Настоящият договор е сключен на {request.ContractDate:dd.MM.yyyy} г.
                    </div>

                    <div class='signature-block'>
                        <div>
                            <p>ЗА СТРАНА А:</p>
                            <div class='signature-line'></div>
                            <p>[Име и подпис]</p>
                        </div>
                        <div>
                            <p>ЗА СТРАНА Б:</p>
                            <div class='signature-line'></div>
                            <p>[Име и подпис]</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>
            ");

            return htmlBuilder.ToString();
        }

    
        private string ConvertPlainTextToHtml(string plainText)
        {
            StringBuilder html = new StringBuilder();
            StringReader reader = new StringReader(plainText);
            string line;
            bool inList = false;
            int currentIndentLevel = 0; 

            
            string ProcessFormatting(string text)
            {
              
                text = text.Replace("[ПОПЪЛНЕТЕ ТУК]", "<u>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</u>");
              
                text = Regex.Replace(text, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
                
                text = Regex.Replace(text, @"__(.*?)__", "<u>$1</u>");
             
                text = Regex.Replace(text, @"_(.*?)_", "<i>$1</i>");
             
                text = text.Replace("[ПОЛЕТА В КВАДРАТНИ СКОБИ]", "<u><strong>ПОЛЕТА В КВАДРАТНИ СКОБИ</strong></u>");
                return text;
            }

            while ((line = reader.ReadLine()) != null)
            {
                string trimmedLine = line.TrimStart(); 
                int leadingSpaces = line.Length - trimmedLine.Length;

              
                string processedLineContent = ProcessFormatting(trimmedLine);

             
                if (processedLineContent.StartsWith("ТРУДОВ ДОГОВОР") || processedLineContent.StartsWith("ДОГОВОР ЗА НАЕМ") ||
                    processedLineContent.StartsWith("ДОГОВОР ЗА УСЛУГА") || processedLineContent.StartsWith("ДОГОВОР ЗА ПОКУПКО-ПРОДАЖБА"))
                {
                   
                    while (currentIndentLevel > 0) { html.Append("</ul>"); currentIndentLevel -= 2; }
                    if (inList) { html.Append("</ul>"); inList = false; }
                    html.Append($"<h1>{processedLineContent}</h1>");
                }
              
                else if (processedLineContent.StartsWith("---"))
                {
                    
                    while (currentIndentLevel > 0) { html.Append("</ul>"); currentIndentLevel -= 2; }
                    if (inList) { html.Append("</ul>"); inList = false; }
                    html.Append("<hr/>");
                }
             
                else if (Regex.IsMatch(processedLineContent, @"^\d+\.\s"))
                {
                  
                    while (currentIndentLevel > 0) { html.Append("</ul>"); currentIndentLevel -= 2; }
                    if (inList) { html.Append("</ul>"); inList = false; }
                    html.Append($"<h2>{processedLineContent}</h2>");
                }
               
                else if (trimmedLine.StartsWith("- "))
                {
                   
                    int newIndentLevel = leadingSpaces;

                    
                    while (currentIndentLevel < newIndentLevel)
                    {
                        html.Append("<ul>");
                        currentIndentLevel += 2; 
                    }
                    while (currentIndentLevel > newIndentLevel)
                    {
                        html.Append("</ul>");
                        currentIndentLevel -= 2; 
                    }

                    if (!inList) 
                    {
                        html.Append("<ul>");
                        inList = true;
                    }

                    string listItemContent = processedLineContent.Substring(2); 
                    html.Append($"<li>{listItemContent}</li>");
                }
              
                else
                {
                    while (currentIndentLevel > 0) { html.Append("</ul>"); currentIndentLevel -= 2; }
                    if (inList) { html.Append("</ul>"); inList = false; }

                    if (!string.IsNullOrWhiteSpace(processedLineContent))
                    {
                        html.Append($"<p>{processedLineContent}</p>");
                    }
                }
            }

           
            while (currentIndentLevel > 0) { html.Append("</ul>"); currentIndentLevel -= 2; }
            if (inList) { html.Append("</ul>"); }

            return html.ToString();
        }
    }
}
