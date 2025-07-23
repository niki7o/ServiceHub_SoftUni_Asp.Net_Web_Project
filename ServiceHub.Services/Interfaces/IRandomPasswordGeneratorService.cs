using ServiceHub.Core.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IRandomPasswordGeneratorService
    {
        Task<PasswordGenerateResponseModel> GeneratePasswordAsync(PasswordGenerateRequestModel request);

    }
}
