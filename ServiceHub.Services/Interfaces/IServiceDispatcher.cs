using ServiceHub.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Services.Interfaces
{
    public interface IServiceDispatcher
    {
        Task<BaseServiceResponse> DispatchAsync(BaseServiceRequest request);

    }
}
