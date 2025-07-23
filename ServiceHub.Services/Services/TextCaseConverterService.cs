using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceHub.Core.Models.Tools;
using ServiceHub.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services
{
    public class TextCaseConverterService : ITextCaseConverterService
    {
        private readonly ILogger<TextCaseConverterService> _logger;

        public TextCaseConverterService(ILogger<TextCaseConverterService> logger)
        {
            _logger = logger;
        }

        public Task<TextCaseConvertResponseModel> ConvertCaseAsync(TextCaseConvertRequestModel request)
        {
            string convertedText = "";
            string message = "";
            bool isSuccess = true;

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Task.FromResult(new TextCaseConvertResponseModel { ConvertedText = "", Message = "Моля, въведете текст.", IsSuccess = false });
            }

            switch (request.CaseType.ToLower())
            {
                case "uppercase":
                    convertedText = request.Text.ToUpper();
                    message = "Текстът е конвертиран в главни букви.";
                    break;
                case "lowercase":
                    convertedText = request.Text.ToLower();
                    message = "Текстът е конвертиран в малки букви.";
                    break;
                case "titlecase":
                    TextInfo textInfo = new CultureInfo("bg-BG", false).TextInfo;
                    convertedText = textInfo.ToTitleCase(request.Text.ToLower());
                    message = "Текстът е конвертиран в заглавен регистър.";
                    break;
                default:
                    _logger.LogWarning("Invalid case type provided: {CaseType}", request.CaseType);
                    message = "Невалиден тип конверсия. Поддържат се 'uppercase', 'lowercase', 'titlecase'.";
                    isSuccess = false;
                    break;
            }

            _logger.LogInformation("Text converted to {CaseType}: Original='{Original}', Converted='{Converted}'", request.CaseType, request.Text, convertedText);
            return Task.FromResult(new TextCaseConvertResponseModel { ConvertedText = convertedText, Message = message, IsSuccess = isSuccess });
        }
    }
}


