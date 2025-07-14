using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Service.FileConverter
{
    public class FileConvertResult: BaseServiceResponse
    {
        public byte[] ConvertedFileContent { get; set; } = null!; 
        public string ConvertedFileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;

    }
    
}
