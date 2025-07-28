using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Service.FileConverter
{
    public class FileConvertResult : BaseServiceResponse
    {
        public byte[]? ConvertedFileContent { get; set; }
        public string? ConvertedFileName { get; set; }
        public string? ContentType { get; set; }
        public string OriginalFileName { get; set; } = null!;
        public string TargetFormat { get; set; } = null!;
    }

}
