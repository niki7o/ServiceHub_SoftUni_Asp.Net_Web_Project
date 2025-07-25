using Microsoft.Extensions.Logging;
using ServiceHub.Common;
using ServiceHub.Core.Models;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Html2pdf;
using iText.Html2pdf.Exceptions;
using iText.Bouncycastleconnector;


namespace ServiceHub.Services.Services
{
    public class CvGeneratorService : ICvGeneratorService
    {
        private readonly ILogger<CvGeneratorService> _logger;

        public CvGeneratorService(ILogger<CvGeneratorService> logger)
        {
            _logger = logger;
        }

        
        public async Task<CvGenerateResult> GenerateCvAsync(CvGenerateRequestModel request)
        {
            _logger.LogInformation("Attempting to generate CV for {Name} from HTML template to PDF.", request.Name);

            try
            {
                string htmlContent = GenerateCvHtml(request);
                _logger.LogDebug("Generated HTML Content (first 500 chars): {HtmlSnippet}", htmlContent.Substring(0, Math.Min(htmlContent.Length, 500)));

                using (MemoryStream generatedCvStream = new MemoryStream())
                {
                    ConverterProperties converterProperties = new ConverterProperties();
                    // converterProperties.SetBaseUri("http://localhost:7185/"); // Може да помогне за зареждане на външни ресурси, ако има такива

                    HtmlConverter.ConvertToPdf(htmlContent, generatedCvStream, converterProperties);

                    _logger.LogInformation("CV successfully generated as PDF from HTML for {Name}.", request.Name);
                    return new CvGenerateResult
                    {
                        IsSuccess = true,
                        GeneratedFileContent = generatedCvStream.ToArray(),
                        GeneratedFileName = $"{request.Name}_CV.pdf",
                        ContentType = "application/pdf"
                    };
                }
            }
            catch (Html2PdfException htmlEx)
            {
                _logger.LogError(htmlEx, "Html2PdfException occurred during CV generation for {Name}: {Message}", request.Name, htmlEx.Message);
                return new CvGenerateResult { IsSuccess = false, ErrorMessage = $"Грешка при конвертиране на HTML в PDF: {htmlEx.Message}" };
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

            htmlBuilder.Append(@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <title>CV на " + (request.Name ?? "Кандидат") + @"</title>
                <style>
                    body { font-family: 'Arial', sans-serif; line-height: 1.6; color: #333; margin: 20px; background-color: #ffffff; }
                    .container { max-width: 800px; margin: auto; background: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
                    h1, h2, h3 { color: #0056b3; }
                    .header { text-align: center; margin-bottom: 20px; }
                    .header h1 { margin: 0; font-size: 2.5em; }
                    .header p { margin: 5px 0; color: #555; }
                    .section { margin-bottom: 20px; border-bottom: 1px solid #eee; padding-bottom: 10px; }
                    .section:last-child { border-bottom: none; }
                    .section h2 { font-size: 1.5em; border-bottom: 2px solid #0056b3; padding-bottom: 5px; margin-bottom: 15px; }
                    ul { list-style-type: none; padding: 0; }
                    ul li { margin-bottom: 5px; }
                    .contact-info { display: flex; justify-content: center; gap: 20px; margin-top: 10px; }
                    .contact-info div { display: flex; align-items: center; }
                    .contact-info i { margin-right: 8px; color: #0056b3; }
                    .summary { margin-top: 20px; font-style: italic; color: #666; }
                    .experience-item, .education-item { margin-bottom: 10px; }
                    .job-title, .degree-title { font-weight: bold; color: #0056b3; }
                    .company-city, .university-city { font-style: italic; color: #777; }
                    .dates { float: right; color: #888; }
                    .skills-list { display: flex; flex-wrap: wrap; gap: 10px; }
                    .skill-item { background-color: #e0e0e0; padding: 5px 10px; border-radius: 5px; }
                </style>
                <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css' crossorigin='anonymous' referrerpolicy='no-referrer' />
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>" + (request.Name ?? "Вашето Име") + @"</h1>
                        <p>Резюме / Длъжност</p>
                        <div class='contact-info'>
                            <div><i class='fas fa-envelope'></i> " + (request.Email ?? "email@example.com") + @"</div>
                            <div><i class='fas fa-phone'></i> " + (request.Phone ?? "Няма телефон") + @"</div>
                        </div>
                    </div>
            ");

            htmlBuilder.Append(@"
                    <div class='section summary'>
                        <h2>Резюме</h2>
                        <p>Кратко обобщение на вашия опит и умения.</p>
                    </div>
            ");

            if (!string.IsNullOrWhiteSpace(request.Experience))
            {
                htmlBuilder.Append(@"
                    <div class='section'>
                        <h2>ПРОФЕСИОНАЛЕН ОПИТ</h2>
                        <div class='experience-item'>
                            <div class='job-title'>Примерна Длъжност</div>
                            <div class='company-city'>Примерна Компания, Град</div>
                            <div class='dates'>Ян 2023 - Дек 2023</div>
                            <p>" + (request.Experience ?? "") + @"</p>
                        </div>
                    </div>
                ");
            }

            if (!string.IsNullOrWhiteSpace(request.Education))
            {
                htmlBuilder.Append(@"
                    <div class='section'>
                        <h2>ОБРАЗОВАНИЕ</h2>
                        <div class='education-item'>
                            <div class='degree-title'>Примерна Степен / Специалност</div>
                            <div class='university-city'>Примерен Университет, Град</div>
                            <div class='dates'>Ян 2023 - Дек 2023</div>
                            <p>" + (request.Education ?? "") + @"</p>
                        </div>
                    </div>
                ");
            }

            if (!string.IsNullOrWhiteSpace(request.Skills))
            {
                htmlBuilder.Append(@"
                    <div class='section'>
                        <h2>УМЕНИЯ</h2>
                        <div class='skills-list'>");
                foreach (var skill in request.Skills.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    htmlBuilder.Append($"<span class='skill-item'>{skill.Trim()}</span>");
                }
                htmlBuilder.Append(@"
                        </div>
                    </div>
                ");
            }

            htmlBuilder.Append(@"
                </div>
            </body>
            </html>
            ");

            return htmlBuilder.ToString();
        }
    }
}

