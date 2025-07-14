using ServiceHub.Common;

namespace ServiceHub.Services.Services
{
    internal class ServiceErrorResponse : BaseServiceResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
}