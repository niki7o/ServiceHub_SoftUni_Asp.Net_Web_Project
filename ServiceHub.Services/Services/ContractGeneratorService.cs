using iText.Html2pdf;
using iText.Html2pdf.Exceptions;
using Microsoft.Extensions.Logging;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services
{
    public class ContractGeneratorService : IContractGeneratorService
    {
        private readonly ILogger<ContractGeneratorService> _logger;

        public ContractGeneratorService(ILogger<ContractGeneratorService> logger)
        {
            _logger = logger;
        }

        public async Task<ContractGenerateResult> GenerateContractAsync(ContractGenerateRequestModel request)
        {
            _logger.LogInformation("Attempting to generate contract for {ContractType} between {PartyA} and {PartyB}.", request.ContractType, request.PartyA, request.PartyB);

            try
            {
                string htmlContent = GenerateContractHtml(request);
                _logger.LogDebug("Generated HTML Content (first 500 chars): {HtmlSnippet}", htmlContent.Substring(0, Math.Min(htmlContent.Length, 500)));

                using (MemoryStream generatedContractStream = new MemoryStream())
                {
                    ConverterProperties converterProperties = new ConverterProperties();
                    HtmlConverter.ConvertToPdf(htmlContent, generatedContractStream, converterProperties);

                    _logger.LogInformation("Contract successfully generated as PDF for {ContractType}.", request.ContractType);
                    return new ContractGenerateResult
                    {
                        IsSuccess = true,
                        GeneratedFileContent = generatedContractStream.ToArray(),
                        GeneratedFileName = $"{request.ContractType}_{request.PartyA}_{request.PartyB}_{DateTime.Now:yyyyMMdd}.pdf",
                        ContentType = "application/pdf"
                    };
                }
            }
            catch (Html2PdfException htmlEx)
            {
                _logger.LogError(htmlEx, "Html2PdfException occurred during contract generation for {ContractType}: {Message}", request.ContractType, htmlEx.Message);
                return new ContractGenerateResult { IsSuccess = false, ErrorMessage = $"Грешка при конвертиране на HTML в PDF за договор: {htmlEx.Message}" };
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

            htmlBuilder.Append($@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>Договор за {request.ContractType ?? "Услуга"}</title>
                <style>
                    body {{ font-family: 'Arial', sans-serif; line-height: 1.6; color: #333; margin: 40px; background-color: #f9f9f9; }}
                    .container {{ max-width: 800px; margin: auto; background: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 0 15px rgba(0,0,0,0.1); }}
                    h1, h2, h3 {{ color: #0056b3; text-align: center; }}
                    .header {{ text-align: center; margin-bottom: 30px; }}
                    .header h1 {{ margin: 0; font-size: 2.2em; }}
                    .party-info {{ margin-bottom: 20px; border: 1px solid #eee; padding: 15px; border-radius: 5px; }}
                    .party-info p {{ margin: 5px 0; }}
                    .section {{ margin-bottom: 25px; }}
                    .section h2 {{ font-size: 1.4em; border-bottom: 2px solid #0056b3; padding-bottom: 8px; margin-bottom: 15px; }}
                    .terms {{ white-space: pre-wrap; background-color: #f0f8ff; padding: 15px; border-radius: 5px; border: 1px dashed #aaddff; }}
                    .signature-block {{ margin-top: 40px; display: flex; justify-content: space-around; text-align: center; }}
                    .signature-line {{ border-top: 1px solid #aaa; width: 40%; padding-top: 5px; }}
                    .date-info {{ text-align: right; margin-top: 30px; font-style: italic; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>ДОГОВОР ЗА {request.ContractType?.ToUpper() ?? "УСЛУГА"}</h1>
                        <p>Дата: {request.ContractDate:dd.MM.yyyy}</p>
                    </div>

                    <div class='section'>
                        <h2>СТРАНИ</h2>
                        <div class='party-info'>
                            <h3>СТРАНА А:</h3>
                            <p><strong>{request.PartyA}</strong></p>
                            <p><i>(Представлявана от: [Име на представител], [Длъжност])</i></p>
                        </div>
                        <div class='party-info'>
                            <h3>СТРАНА Б:</h3>
                            <p><strong>{request.PartyB}</strong></p>
                            <p><i>(Представлявана от: [Име на представител], [Длъжност])</i></p>
                        </div>
                    </div>

                    <div class='section'>
                        <h2>ПРЕДМЕТ НА ДОГОВОРА</h2>
                        <p>Настоящият договор се сключва за {request.ContractType?.ToLower()} между горепосочените страни.</p>
                    </div>

                    <div class='section'>
                        <h2>УСЛОВИЯ НА ДОГОВОРА</h2>
                        <div class='terms'>
                            {request.ContractTerms}
                        </div>
                    </div>
            ");

            if (!string.IsNullOrWhiteSpace(request.AdditionalInfo))
            {
                htmlBuilder.Append($@"
                    <div class='section'>
                        <h2>ДОПЪЛНИТЕЛНА ИНФОРМАЦИЯ</h2>
                        <p>{request.AdditionalInfo}</p>
                    </div>
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
    }
}
