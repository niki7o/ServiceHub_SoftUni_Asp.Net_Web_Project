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
    public class RandomPasswordGeneratorService : IRandomPasswordGeneratorService
    {
         private readonly ILogger<RandomPasswordGeneratorService> _logger;
            private static readonly Random _random = new Random();

            private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            private const string DigitChars = "0123456789";
            private const string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            public RandomPasswordGeneratorService(ILogger<RandomPasswordGeneratorService> logger)
            {
                _logger = logger;
            }

            public Task<PasswordGenerateResponseModel> GeneratePasswordAsync(PasswordGenerateRequestModel request)
            {
                // Validation (can also be done with Data Annotations on the model)
                if (request.Length < 8 || request.Length > 32)
                {
                    return Task.FromResult(new PasswordGenerateResponseModel
                    {
                        GeneratedPassword = "",
                        Message = "Дължината на паролата трябва да е между 8 и 32 символа."
                    });
                }

                if (!request.IncludeUppercase && !request.IncludeLowercase && !request.IncludeDigits && !request.IncludeSpecialChars)
                {
                    return Task.FromResult(new PasswordGenerateResponseModel
                    {
                        GeneratedPassword = "",
                        Message = "Моля, изберете поне един тип символи за генериране на парола."
                    });
                }

                var charPool = new StringBuilder();
                if (request.IncludeUppercase) charPool.Append(UppercaseChars);
                if (request.IncludeLowercase) charPool.Append(LowercaseChars);
                if (request.IncludeDigits) charPool.Append(DigitChars);
                if (request.IncludeSpecialChars) charPool.Append(SpecialChars);

                if (charPool.Length == 0)
                {
                    return Task.FromResult(new PasswordGenerateResponseModel
                    {
                        GeneratedPassword = "",
                        Message = "Не може да се генерира парола без избран набор от символи."
                    });
                }

                var passwordBuilder = new StringBuilder();
                for (int i = 0; i < request.Length; i++)
                {
                    int index = _random.Next(charPool.Length);
                    passwordBuilder.Append(charPool[index]);
                }

                string generatedPassword = passwordBuilder.ToString();
                _logger.LogInformation("Generated password of length {Length} with selected options.", request.Length);

                return Task.FromResult(new PasswordGenerateResponseModel
                {
                    GeneratedPassword = generatedPassword,
                    Message = "Паролата е генерирана успешно."
                });
            }
        
    }
}
