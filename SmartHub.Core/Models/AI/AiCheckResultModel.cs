using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.AI
{
    public class AiCheckResultModel
    {
        public bool IsSuccessful { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string CorrectedText { get; set; } = string.Empty; 
        public List<AiErrorDetail> Errors { get; set; } = new List<AiErrorDetail>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
