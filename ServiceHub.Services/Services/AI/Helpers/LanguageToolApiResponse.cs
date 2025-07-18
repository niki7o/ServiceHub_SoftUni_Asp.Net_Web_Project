using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Services.AI.Helpers
{
    public class LanguageToolApiResponse
    {
        public List<LanguageToolMatch> Matches { get; set; } = new List<LanguageToolMatch>();
    }
}
