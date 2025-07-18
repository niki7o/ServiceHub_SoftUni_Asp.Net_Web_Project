using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services.AI.Helpers
{
    public class LanguageToolRule
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public LanguageToolRuleCategory? Category { get; set; }
    }
}
