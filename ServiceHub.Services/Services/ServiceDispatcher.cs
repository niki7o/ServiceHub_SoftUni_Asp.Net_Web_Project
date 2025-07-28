using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceHub.Common;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Services.Services
{
    public class ServiceDispatcher : IServiceDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceDispatcher> _logger;
        private readonly Dictionary<Guid, Type> _serviceImplementations;

        public ServiceDispatcher(
            IServiceProvider serviceProvider,
            ILogger<ServiceDispatcher> logger,
            Dictionary<Guid, Type> serviceImplementations)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _serviceImplementations = serviceImplementations;
        }

        public async Task<BaseServiceResponse> DispatchAsync(BaseServiceRequest request)
        {
            if (!_serviceImplementations.TryGetValue(request.ServiceId, out var serviceInterfaceType))
            {
                _logger.LogWarning($"Service with ID '{request.ServiceId}' not found in dispatcher mapping.");
                return new ServiceErrorResponse { IsSuccess = false, ErrorMessage = $"Услуга с ID '{request.ServiceId}' не е намерена." };
            }

            var service = _serviceProvider.GetRequiredService(serviceInterfaceType) as IExecutableService;

            if (service == null)
            {
                _logger.LogError($"Could not resolve service implementation for type '{serviceInterfaceType.Name}' or it does not implement IExecutableService.");
                return new ServiceErrorResponse { IsSuccess = false, ErrorMessage = "Неуспешно зареждане на услугата или не поддържа основния интерфейс за изпълнение." };
            }

            _logger.LogInformation($"Dispatching request for service: {request.ServiceId}");

            return await service.ExecuteAsync(request);
        }
    }
}
