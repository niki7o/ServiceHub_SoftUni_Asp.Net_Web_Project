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
    public class WordCharacterCounterService : IWordCharacterCounterService
    {
        private readonly ILogger<WordCharacterCounterService> _logger;

        public WordCharacterCounterService(ILogger<WordCharacterCounterService> logger)
        {
            _logger = logger;
        }

        public Task<WordCharacterCountResponseModel> CountTextAsync(WordCharacterCountRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Task.FromResult(new WordCharacterCountResponseModel
                {
                    WordCount = 0,
                    CharacterCount = 0,
                    LineCount = 0
                });
            }

            int wordCount = 0;
            int characterCount = request.Text.Length;
            int lineCount = request.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length;

           
            wordCount = request.Text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

            _logger.LogInformation("Text counted: Words={WordCount}, Chars={CharCount}, Lines={LineCount}", wordCount, characterCount, lineCount);

            return Task.FromResult(new WordCharacterCountResponseModel
            {
                WordCount = wordCount,
                CharacterCount = characterCount,
                LineCount = lineCount
            });
        }
    }
}

