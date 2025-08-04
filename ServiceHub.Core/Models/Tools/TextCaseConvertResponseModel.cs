using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class TextCaseConvertResponseModel
    {
        public string ConvertedText { get; set; } = null!;
        public string? Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
