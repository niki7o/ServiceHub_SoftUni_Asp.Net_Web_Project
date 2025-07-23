using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class PasswordGenerateRequestModel
    {
        [Range(8, 32, ErrorMessage = "Дължината на паролата трябва да е между 8 и 32 символа.")]
        public int Length { get; set; } = 12; 

        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeDigits { get; set; } = true;
        public bool IncludeSpecialChars { get; set; } = true;
    }
}
