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
        private static readonly Random random = new Random(); // Използваме статичен Random за по-добро разпределение

        private const string lower = "abcdefghijklmnopqrstuvwxyz";
        private const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string digits = "0123456789";
        private const string special = "!@#$%^&*()_+{}[]:;<>,.?/~";

        public RandomPasswordGeneratorService(ILogger<RandomPasswordGeneratorService> logger)
        {
            _logger = logger;
        }

        public Task<PasswordGenerateResponseModel> GeneratePasswordAsync(PasswordGenerateRequestModel request)
        {
            // Валидация, която може да се дублира и в контролера чрез ModelState.IsValid
            if (request.Length < 1 || request.Length > 128)
            {
                _logger.LogWarning("Invalid password length requested: {Length}", request.Length);
                return Task.FromResult(new PasswordGenerateResponseModel { GeneratedPassword = "", Message = "Дължината на паролата трябва да е между 1 и 128 символа." });
            }

            var chars = new List<char>();

            if (request.IncludeLowercase) chars.AddRange(lower);
            if (request.IncludeUppercase) chars.AddRange(upper);
            if (request.IncludeDigits) chars.AddRange(digits);
            if (request.IncludeSpecialChars) chars.AddRange(special);

            if (chars.Count == 0)
            {
                _logger.LogWarning("No character types selected for password generation. Defaulting to lowercase.");
                chars.AddRange(lower);
            }

            var passwordBuilder = new StringBuilder();
            for (int i = 0; i < request.Length; i++)
            {
                passwordBuilder.Append(chars[random.Next(chars.Count)]);
            }

            string generatedPassword = passwordBuilder.ToString();
            _logger.LogInformation("Generated password of length {Length}.", request.Length);

            return Task.FromResult(new PasswordGenerateResponseModel { GeneratedPassword = generatedPassword, Message = "Паролата е генерирана успешно." });
        }
    }
}
