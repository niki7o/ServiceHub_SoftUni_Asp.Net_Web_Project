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
    public class FinancialCalculatorService : IFinancialCalculatorService
    {
        private readonly ILogger<FinancialCalculatorService> _logger;
        private readonly IConverter _converter;

        public FinancialCalculatorService(ILogger<FinancialCalculatorService> logger, IConverter converter)
        {
            _logger = logger;
            _converter = converter;
        }

        public async Task<FinancialCalculationResult> CalculateFinancialsAsync(FinancialCalculatorRequestModel request)
        {
            _logger.LogInformation("Attempting financial calculation.");

            try
            {
                var result = new FinancialCalculationResult { IsSuccess = true };

               
                if (request.CostOfInvestment > 0)
                {
                    result.CalculatedROI = (request.NetProfitForROI / request.CostOfInvestment) * 100;
                }
                else
                {
                    result.CalculatedROI = 0; 
                }

               
                result.TotalRevenues = request.Revenues.Sum(r => r.Amount);
                result.TotalExpenses = request.Expenses.Sum(e => e.Amount);
                result.NetProfitLoss = result.TotalRevenues - result.TotalExpenses;

               
                decimal forecastedRevenue = result.TotalRevenues;
                decimal forecastedExpenses = result.TotalExpenses;

                if (request.GrowthRatePercentage > 0 && request.ForecastPeriodMonths > 0)
                {
                    
                    decimal monthlyGrowthFactor = 1 + (request.GrowthRatePercentage / 100);

                    for (int i = 0; i < request.ForecastPeriodMonths; i++)
                    {
                        forecastedRevenue *= monthlyGrowthFactor;
                        forecastedExpenses *= monthlyGrowthFactor;
                    }
                }
                result.ForecastedRevenues = forecastedRevenue;
                result.ForecastedExpenses = forecastedExpenses;
                result.ForecastedNetProfitLoss = forecastedRevenue - forecastedExpenses;


                string htmlContent = GenerateFinancialReportHtml(request, result);
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

                result.GeneratedFileContent = pdf;
                result.GeneratedFileName = $"FinancialReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                result.ContentType = "application/pdf";

                _logger.LogInformation("Financial report successfully generated as PDF.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error occurred during financial calculation: {Message}", ex.Message);
                return new FinancialCalculationResult { IsSuccess = false, ErrorMessage = $"Възникна грешка при изчисляване: {ex.Message}" };
            }
        }

        private string GenerateFinancialReportHtml(FinancialCalculatorRequestModel request, FinancialCalculationResult result)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.Append($@"
            <!DOCTYPE html>
            <html lang='bg'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Финансов Отчет</title>
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
                    .report-container {{
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
                    h2 {{
                        color: #0056b3;
                        border-bottom: 1px solid #007bff;
                        padding-bottom: 8px;
                        margin-top: 30px;
                        margin-bottom: 20px;
                        font-size: 1.6em;
                    }}
                    .section-block {{
                        padding: 15px;
                        border: 1px solid #eee;
                        border-radius: 8px;
                        background-color: #f8f8f8;
                        margin-bottom: 20px;
                    }}
                    .section-block p {{
                        margin: 5px 0;
                    }}
                    .section-block strong {{
                        color: #333;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 20px;
                    }}
                    table th, table td {{
                        border: 1px solid #ddd;
                        padding: 10px;
                        text-align: left;
                    }}
                    table th {{
                        background-color: #007bff;
                        color: #fff;
                        font-weight: bold;
                        font-size: 0.9em;
                    }}
                    table tr:nth-child(even) {{
                        background-color: #f2f2f2;
                    }}
                    .summary-table {{
                        width: 100%;
                        margin-top: 20px;
                        border: none;
                    }}
                    .summary-table td {{
                        border: none;
                        padding: 8px 0;
                    }}
                    .summary-table .label {{
                        font-weight: bold;
                        color: #0056b3;
                        width: 70%;
                    }}
                    .summary-table .value {{
                        text-align: right;
                        font-weight: bold;
                        color: #333;
                    }}
                    .summary-table .net-profit {{
                        font-size: 1.2em;
                        color: #28a745; /* Green for profit */
                    }}
                    .summary-table .net-loss {{
                        font-size: 1.2em;
                        color: #dc3545; /* Red for loss */
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
                <div class='report-container'>
                    <div class='header'>
                        <h1>Финансов Отчет</h1>
                        <p>Дата на генериране: <strong>{DateTime.Now:dd.MM.yyyy HH:mm}</strong></p>
                    </div>

                    <h2>1. Анализ на възвръщаемост на инвестициите (ROI)</h2>
                    <div class='section-block'>
                        <p>Нетна печалба за ROI: <strong>{request.NetProfitForROI:F2} лв.</strong></p>
                        <p>Цена на инвестицията: <strong>{request.CostOfInvestment:F2} лв.</strong></p>
                        <p><strong>Изчислен ROI: <span style='color: {(result.CalculatedROI >= 0 ? "#28a745" : "#dc3545")};'>{result.CalculatedROI:F2}%</span></strong></p>
                    </div>

                    <h2>2. Бюджет / Отчет за приходи и разходи</h2>
                    <div class='section-block'>
                        <h3>Приходи</h3>
                        {(request.Revenues.Any() ?
                            "<table><thead><tr><th>Описание</th><th>Сума</th></tr></thead><tbody>" +
                            string.Join("", request.Revenues.Select(r => $"<tr><td>{r.Description}</td><td>{r.Amount:F2} лв.</td></tr>")) +
                            "</tbody></table>"
                            : "<p>Няма въведени приходи.</p>")}

                        <h3>Разходи</h3>
                        {(request.Expenses.Any() ?
                            "<table><thead><tr><th>Описание</th><th>Сума</th></tr></thead><tbody>" +
                            string.Join("", request.Expenses.Select(e => $"<tr><td>{e.Description}</td><td>{e.Amount:F2} лв.</td></tr>")) +
                            "</tbody></table>"
                            : "<p>Няма въведени разходи.</p>")}

                        <table class='summary-table'>
                            <tr><td class='label'>Общи приходи:</td><td class='value'>{result.TotalRevenues:F2} лв.</td></tr>
                            <tr><td class='label'>Общи разходи:</td><td class='value'>{result.TotalExpenses:F2} лв.</td></tr>
                            <tr><td class='label'>Нетна печалба/загуба:</td><td class='value {(result.NetProfitLoss >= 0 ? "net-profit" : "net-loss")}'>{result.NetProfitLoss:F2} лв.</td></tr>
                        </table>
                    </div>

                    <h2>3. Прогноза</h2>
                    <div class='section-block'>
                        <p>Процент на растеж (на период): <strong>{request.GrowthRatePercentage:F2}%</strong></p>
                        <p>Период на прогноза: <strong>{request.ForecastPeriodMonths} месеца</strong></p>
                        <table class='summary-table'>
                            <tr><td class='label'>Прогнозирани приходи:</td><td class='value'>{result.ForecastedRevenues:F2} лв.</td></tr>
                            <tr><td class='label'>Прогнозирани разходи:</td><td class='value'>{result.ForecastedExpenses:F2} лв.</td></tr>
                            <tr><td class='label'>Прогнозирана нетна печалба/загуба:</td><td class='value {(result.ForecastedNetProfitLoss >= 0 ? "net-profit" : "net-loss")}'>{result.ForecastedNetProfitLoss:F2} лв.</td></tr>
                        </table>
                    </div>

                    {(string.IsNullOrWhiteSpace(request.Notes) ? "" : $@"
                    <div class='notes-section'>
                        <h3>Бележки:</h3>
                        <p>{request.Notes.Replace(Environment.NewLine, "<br/>")}</p>
                    </div>")}

                    <div class='footer-section'>
                        <p>Този отчет е генериран от Финансовия Калкулатор / Анализатор.</p>
                        <p><i>Моля, имайте предвид, че това е автоматизиран отчет и не замества професионална финансова консултация.</i></p>
                    </div>
                </div>
            </body>
            </html>
            ");

            return htmlBuilder.ToString();
        }
    }
}
