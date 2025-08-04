using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class CodeSnippetConvertRequestModel
    {
        [Required(ErrorMessage = "Изходният код е задължителен.")]
        public string SourceCode { get; set; } = null!;

        [Required(ErrorMessage = "Изходният език е задължителен.")]
        public string SourceLanguage { get; set; } = null!;

        [Required(ErrorMessage = "Целевият език е задължителен.")]
        public string TargetLanguage { get; set; } = null!;
    }
}
