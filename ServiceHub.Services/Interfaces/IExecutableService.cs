using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IExecutableService
    {

        Task<BaseServiceResponse> ExecuteAsync(BaseServiceRequest request);
    }
}
