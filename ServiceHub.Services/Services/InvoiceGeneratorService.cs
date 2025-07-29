using DinkToPdf;
using DinkToPdf.Contracts;
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
    public class InvoiceGeneratorService : IInvoiceGeneratorService
    {
        private readonly ILogger<InvoiceGeneratorService> _logger;
        private readonly IConverter _converter;

        public InvoiceGeneratorService(ILogger<InvoiceGeneratorService> logger, IConverter converter)
        {
            _logger = logger;
            _converter = converter;
        }

        public async Task<InvoiceGenerateResult> GenerateInvoiceAsync(InvoiceGenerateRequestModel request)
        {
            _logger.LogInformation("Attempting to generate invoice {InvoiceNumber} for {SellerName} to {BuyerName}.", request.InvoiceNumber, request.SellerName, request.BuyerName);

            try
            {
                
                decimal subtotal = request.Items.Sum(item => item.Quantity * item.UnitPrice);
                decimal discountAmount = subtotal * (request.DiscountPercentage / 100);
                decimal taxableAmount = subtotal - discountAmount;
                decimal taxAmount = taxableAmount * (request.TaxRatePercentage / 100);
                decimal totalAmount = taxableAmount + taxAmount;

                string htmlContent = GenerateInvoiceHtml(request, subtotal, discountAmount, taxAmount, totalAmount);
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

                _logger.LogInformation("Invoice {InvoiceNumber} successfully generated as PDF.", request.InvoiceNumber);
                return new InvoiceGenerateResult
                {
                    IsSuccess = true,
                    GeneratedFileContent = pdf,
                    GeneratedFileName = $"Invoice_{request.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf",
                    ContentType = "application/pdf"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error occurred during invoice generation for {InvoiceNumber}: {Message}", request.InvoiceNumber, ex.Message);
                return new InvoiceGenerateResult { IsSuccess = false, ErrorMessage = $"Възникна грешка при генериране на фактура: {ex.Message}" };
            }
        }

        private string GenerateInvoiceHtml(InvoiceGenerateRequestModel request, decimal subtotal, decimal discountAmount, decimal taxAmount, decimal totalAmount)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.Append($@"
            <!DOCTYPE html>
            <html lang='bg'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Фактура №{request.InvoiceNumber}</title>
                <style>
                    body {{
                        font-family: 'Arial', 'Times New Roman', 'Verdana', 'Segoe UI', 'Helvetica', sans-serif;
                        line-height: 1.6;
                        color: #333;
                        margin: 0;
                        padding: 0;
                        background-color: #f9f9f9;
                        font-size: 10pt;
                    }}
                    .invoice-container {{
                        max-width: 800px;
                        margin: 30px auto;
                        background: #fff;
                        padding: 40px 50px;
                        border-radius: 10px;
                        box-shadow: 0 5px 20px rgba(0,0,0,0.08);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                        border-bottom: 2px solid #007bff;
                        padding-bottom: 20px;
                    }}
                    .header h1 {{
                        color: #004085;
                        font-size: 2.8em;
                        margin: 0;
                        text-transform: uppercase;
                        letter-spacing: 2px;
                    }}
                    .header p {{
                        margin: 5px 0;
                        font-size: 1.1em;
                        color: #555;
                    }}
                    .details-section {{
                        display: flex;
                        justify-content: space-between;
                        margin-bottom: 30px;
                        flex-wrap: wrap; /* Allow wrapping on smaller screens */
                    }}
                    .details-block {{
                        width: 48%; /* Roughly half width */
                        padding: 15px;
                        border: 1px solid #eee;
                        border-radius: 8px;
                        background-color: #f8f8f8;
                        margin-bottom: 15px; /* Space between blocks on wrap */
                    }}
                    .details-block h3 {{
                        color: #0056b3;
                        margin-top: 0;
                        margin-bottom: 10px;
                        font-size: 1.3em;
                        border-bottom: 1px dashed #ccc;
                        padding-bottom: 5px;
                    }}
                    .details-block p {{
                        margin: 5px 0;
                    }}
                    .details-block strong {{
                        color: #333;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 30px;
                    }}
                    table th, table td {{
                        border: 1px solid #ddd;
                        padding: 12px;
                        text-align: left;
                    }}
                    table th {{
                        background-color: #007bff;
                        color: #fff;
                        font-weight: bold;
                        text-transform: uppercase;
                        font-size: 0.9em;
                    }}
                    table tr:nth-child(even) {{
                        background-color: #f2f2f2;
                    }}
                    .totals-section {{
                        width: 40%; /* Align to right */
                        margin-left: auto;
                        border: 1px solid #ddd;
                        border-radius: 8px;
                        padding: 15px;
                        background-color: #f8f8f8;
                    }}
                    .totals-row {{
                        display: flex;
                        justify-content: space-between;
                        padding: 5px 0;
                        border-bottom: 1px dashed #eee;
                    }}
                    .totals-row:last-child {{
                        border-bottom: none;
                        font-weight: bold;
                        font-size: 1.1em;
                        color: #004085;
                    }}
                    .notes-section {{
                        margin-top: 30px;
                        padding: 15px;
                        border: 1px solid #eee;
                        border-radius: 8px;
                        background-color: #f8f8f8;
                    }}
                    .notes-section p {{
                        margin: 0;
                        font-style: italic;
                        color: #666;
                    }}
                    .footer-section {{
                        margin-top: 50px;
                        text-align: center;
                        font-size: 0.9em;
                        color: #777;
                    }}
                </style>
            </head>
            <body>
                <div class='invoice-container'>
                    <div class='header'>
                        <h1>ФАКТУРА</h1>
                        <p>Номер: <strong>{request.InvoiceNumber}</strong></p>
                        <p>Дата на издаване: <strong>{request.IssueDate:dd.MM.yyyy}</strong></p>
                    </div>

                    <div class='details-section'>
                        <div class='details-block'>
                            <h3>Продавач</h3>
                            <p><strong>{request.SellerName}</strong></p>
                            {(string.IsNullOrWhiteSpace(request.SellerAddress) ? "" : $"<p>Адрес: {request.SellerAddress}</p>")}
                            {(string.IsNullOrWhiteSpace(request.SellerEIK) ? "" : $"<p>ЕИК/БУЛСТАТ: {request.SellerEIK}</p>")}
                            {(string.IsNullOrWhiteSpace(request.SellerMOL) ? "" : $"<p>МОЛ: {request.SellerMOL}</p>")}
                        </div>
                        <div class='details-block'>
                            <h3>Купувач</h3>
                            <p><strong>{request.BuyerName}</strong></p>
                            {(string.IsNullOrWhiteSpace(request.BuyerAddress) ? "" : $"<p>Адрес: {request.BuyerAddress}</p>")}
                            {(string.IsNullOrWhiteSpace(request.BuyerEIK) ? "" : $"<p>ЕИК/БУЛСТАТ: {request.BuyerEIK}</p>")}
                            {(string.IsNullOrWhiteSpace(request.BuyerMOL) ? "" : $"<p>МОЛ: {request.BuyerMOL}</p>")}
                        </div>
                    </div>

                    <table>
                        <thead>
                            <tr>
                                <th>Описание</th>
                                <th>Количество</th>
                                <th>Ед. цена</th>
                                <th>Общо</th>
                            </tr>
                        </thead>
                        <tbody>
            ");

            foreach (var item in request.Items)
            {
                htmlBuilder.Append($@"
                            <tr>
                                <td>{item.Description}</td>
                                <td>{item.Quantity:F2}</td>
                                <td>{item.UnitPrice:F2} лв.</td>
                                <td>{(item.Quantity * item.UnitPrice):F2} лв.</td>
                            </tr>
                ");
            }

            htmlBuilder.Append($@"
                        </tbody>
                    </table>

                    <div class='totals-section'>
                        <div class='totals-row'>
                            <span>Междинна сума:</span>
                            <span>{subtotal:F2} лв.</span>
                        </div>
                        <div class='totals-row'>
                            <span>Отстъпка ({request.DiscountPercentage:F2}%):</span>
                            <span>-{discountAmount:F2} лв.</span>
                        </div>
                        <div class='totals-row'>
                            <span>Данък ({request.TaxRatePercentage:F2}%):</span>
                            <span>+{taxAmount:F2} лв.</span>
                        </div>
                        <div class='totals-row'>
                            <span>Обща сума за плащане:</span>
                            <span>{totalAmount:F2} лв.</span>
                        </div>
                    </div>

                    {(string.IsNullOrWhiteSpace(request.PaymentMethod) ? "" : $@"
                    <div class='notes-section'>
                        <p>Начин на плащане: <strong>{request.PaymentMethod}</strong></p>
                    </div>")}

                    {(string.IsNullOrWhiteSpace(request.Notes) ? "" : $@"
                    <div class='notes-section'>
                        <p>Бележки: {request.Notes}</p>
                    </div>")}

                    <div class='footer-section'>
                        <p>Благодарим Ви за бизнеса!</p>
                        <p><i>Моля, свържете се с нас при въпроси.</i></p>
                    </div>
                </div>
            </body>
            </html>
            ");

            return htmlBuilder.ToString();
        }
    }
}
