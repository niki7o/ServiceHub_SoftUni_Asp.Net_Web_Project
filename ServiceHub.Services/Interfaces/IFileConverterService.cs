using ServiceHub.Core.Models.Service.FileConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IFileConverterService : IExecutableService
    {
        Task<FileConvertResult> ConvertFileSpecificAsync(FileConvertRequest request, bool isPremiumUser);
    }
}
