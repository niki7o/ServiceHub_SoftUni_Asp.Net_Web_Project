using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class WordCharacterCountRequestModel
    {
        [Required(ErrorMessage = "Текстът е задължителен.", AllowEmptyStrings = false)]
        public string Text { get; set; } = null!;
    }
}
