using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class TextCaseConvertRequestModel
    {
        [Required(ErrorMessage = "Текстът е задължителен.")]
        public string Text { get; set; } = null!;

        [Required(ErrorMessage = "Типът на конверсия е задължителен.")]
        public string CaseType { get; set; } = null!; 
    }
}
