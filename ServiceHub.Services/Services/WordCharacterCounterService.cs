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
    public class WordCharacterCounterService : IWordCharacterCounterService 
    {
        private readonly ILogger<WordCharacterCounterService> _logger;

        public WordCharacterCounterService(ILogger<WordCharacterCounterService> logger)
        {
            _logger = logger;
        }

        public Task<WordCharacterCountResponseModel> CountTextAsync(WordCharacterCountRequestModel request)
        {
            _logger.LogInformation("CountTextAsync received request.Text: '{RequestText}' (Length: {TextLength})",
                                   request.Text ?? "[NULL]", request.Text?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                _logger.LogWarning("Attempted to count empty or whitespace text.");
                return Task.FromResult(new WordCharacterCountResponseModel
                {
                    WordCount = 0,
                    CharCount = 0,
                    LineCount = 0,
                    Message = "Въведеният текст е празен."
                });
            }

            int charCount = request.Text.Length;

            int wordCount = Regex.Matches(request.Text, @"\b\w+\b").Count;

            int lineCount = request.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length;

            
            if (lineCount == 0 && !string.IsNullOrEmpty(request.Text))
            {
                lineCount = 1;
            }

            _logger.LogInformation("Text counted: Words={WordCount}, Chars={CharCount}, Lines={LineCount}", wordCount, charCount, lineCount);

            return Task.FromResult(new WordCharacterCountResponseModel
            {
                WordCount = wordCount,
                CharCount = charCount,
                LineCount = lineCount,
                Message = "Преброяването е успешно."
            });
        }
    }
}

