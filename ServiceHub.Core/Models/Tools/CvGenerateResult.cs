using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Tools
{
    public class CvGenerateResult : BaseServiceResponse 
    {
        public byte[] GeneratedFileContent { get; set; } = null!;
        public string GeneratedFileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}
