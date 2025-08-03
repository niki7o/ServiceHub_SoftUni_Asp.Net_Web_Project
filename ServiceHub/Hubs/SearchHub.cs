using Microsoft.AspNetCore.SignalR;
using ServiceHub.Core.Models.Service;
using ServiceHub.Services.Interfaces;

namespace ServiceHub.Hubs
{
    public class SearchHub : Hub
    {
        private readonly IServiceService _serviceService;

        public SearchHub(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

       
        public async Task SearchServices(string searchTerm)
        {
            try
            {
                IEnumerable<ServiceViewModel> services = await _serviceService.SearchServicesByTitleAsync(searchTerm);
                await Clients.Caller.SendAsync("ReceiveSearchResults", services);
            }
            catch (Exception ex)
            {
              
                Console.WriteLine($"Error in SearchHub.SearchServices: {ex.Message}");
                
            }
        }
    }
}
