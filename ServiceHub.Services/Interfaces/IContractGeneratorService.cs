using ServiceHub.Core.Models.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IContractGeneratorService
    {
        Task<ContractGenerateResult> GenerateContractAsync(ContractGenerateRequestModel request);
    }
}
