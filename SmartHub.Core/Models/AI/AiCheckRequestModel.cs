using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.AI
{
    public  class AiCheckRequestModel
    {
        [Required(ErrorMessage = "Text is required.")]
        [MinLength(10, ErrorMessage = "Text must be at least 10 characters long.")]
        [MaxLength(5000, ErrorMessage = "Text cannot exceed 5000 characters.")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required.")]
        [RegularExpression("^(bg|en)$", ErrorMessage = "Supported languages are 'bg' and 'en'.")]
        public string Language { get; set; } = string.Empty;
    }
}
