using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class PasswordGenerateResponseModel
    {
        public string GeneratedPassword { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
