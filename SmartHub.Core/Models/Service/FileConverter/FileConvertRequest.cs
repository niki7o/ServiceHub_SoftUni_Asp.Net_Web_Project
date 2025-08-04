using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Core.Models.Service.FileConverter
{
    public class FileConvertRequest: BaseServiceRequest
    {

        [Required(ErrorMessage = "File content is required.")]
        public byte[] FileContent { get; set; } = null!;

        [Required(ErrorMessage = "Original file name (with extension) is required.")]
        public string OriginalFileName { get; set; } = null!;

        [Required(ErrorMessage = "Target format is required.")]
        public string TargetFormat { get; set; } = null!;

        public bool PerformOCRIfApplicable { get; set; } = false;
        public bool IsPremiumUser { get; set; } = false;
    }
}
