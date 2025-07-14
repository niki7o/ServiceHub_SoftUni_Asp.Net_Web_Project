using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Common
{
    public abstract class BaseServiceResponse
    {
        public bool IsSuccess { get; set; } 
        public string? ErrorMessage { get; set; }
    }
}
