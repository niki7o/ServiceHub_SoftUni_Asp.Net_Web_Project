using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services.AI.Helpers
{
    public class LanguageToolMatch
    {
        public string Message { get; set; } = string.Empty;
        public int Offset { get; set; }
        public int Length { get; set; }
        public List<LanguageToolReplacement> Replacements { get; set; } = new List<LanguageToolReplacement>();
        public LanguageToolRule? Rule { get; set; }
    }
}
