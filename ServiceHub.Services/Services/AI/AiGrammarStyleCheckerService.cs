using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http.Headers;
using ServiceHub.Services.Interfaces;
using ServiceHub.Core.Models.AI;
using ServiceHub.Services.Services.AI.Helpers;

namespace ServiceHub.Services.Services.AI
{
    public class AiGrammarStyleCheckerService : IAiGrammarStyleCheckerService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiGrammarStyleCheckerService> _logger;
        private readonly string _languageToolApiUrl;

        public AiGrammarStyleCheckerService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AiGrammarStyleCheckerService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _languageToolApiUrl = _configuration["AiServices:GrammarCheckerApiUrl"]
                                 ?? "https://api.languagetool.org/v2/"; 

            _httpClient.BaseAddress = new Uri(_languageToolApiUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
             _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_API_KEY");
        }

        public async Task<AiCheckResultModel> CheckGrammarAndStyleAsync(string text, string language)
        {
            var result = new AiCheckResultModel
            {
                OriginalText = text,
                IsSuccessful = false
            };

            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("language", language == "bg" ? "bg-BG" : "en-US"), 
                    new KeyValuePair<string, string>("text", text),
                    new KeyValuePair<string, string>("enabledRules", "MORFOLOGIK_RULE_BG_UNLIMITED") 
                    
                });

                var response = await _httpClient.PostAsync("check", content); 

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var ltResponse = JsonSerializer.Deserialize<LanguageToolApiResponse>(jsonResponse);

                    result.IsSuccessful = true;
                    result.CorrectedText = text; 

                    foreach (var match in ltResponse?.Matches ?? new List<LanguageToolMatch>())
                    {
                        var suggestion = match.Replacements.FirstOrDefault()?.Value ?? "";
                        result.Errors.Add(new AiErrorDetail
                        {
                            Message = match.Message,
                            SuggestedCorrection = suggestion,
                            StartIndex = match.Offset,
                            EndIndex = match.Offset + match.Length,
                            Category = match.Rule?.Category?.Name ?? "General" 
                        });
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                         response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    result.ErrorMessage = "API Key or usage limit exceeded. Please check LanguageTool API documentation.";
                    _logger.LogWarning("LanguageTool API authorization/usage error: {StatusCode}", response.StatusCode);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    result.ErrorMessage = $"LanguageTool API Error: {response.StatusCode} - {errorContent}";
                    _logger.LogError("LanguageTool API call failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                }
            }
            catch (HttpRequestException httpEx)
            {
                result.ErrorMessage = $"Network or API connection error: {httpEx.Message}";
                _logger.LogError(httpEx, "Error connecting to LanguageTool API.");
            }
            catch (JsonException jsonEx)
            {
                result.ErrorMessage = $"Failed to parse LanguageTool API response: {jsonEx.Message}";
                _logger.LogError(jsonEx, "Error parsing LanguageTool API response.");
            }
            catch (System.Exception ex)
            {
                result.ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                _logger.LogError(ex, "Unexpected error in AI grammar and style check.");
            }

            return result;
        }

        






       

     
    
   }
}
