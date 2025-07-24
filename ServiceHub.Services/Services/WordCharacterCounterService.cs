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
        // NEW LOG: Log the incoming request text
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

        // Count characters (including whitespace)
        int charCount = request.Text.Length;

        // More robust word counting using Regex to split by any non-alphanumeric characters
        // and then filtering out empty entries. This handles multiple spaces, punctuation better.
        int wordCount = Regex.Matches(request.Text, @"\b\w+\b").Count;

        // Count lines: Split by newline characters. If the text is not empty, there's at least one line.
        // Handles cases with empty lines or just a single line without a newline at the end.
        int lineCount = request.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                                    .Length;
        // If the text is not empty but contains only newlines, ensure lineCount is at least 1.
        if (string.IsNullOrWhiteSpace(request.Text) && lineCount > 0)
        {
            lineCount = 0; // If only whitespace/newlines, but no actual content, consider 0 lines.
        }
        else if (!string.IsNullOrWhiteSpace(request.Text) && lineCount == 0)
        {
            lineCount = 1; // If there's text but no newlines, it's at least one line.
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
