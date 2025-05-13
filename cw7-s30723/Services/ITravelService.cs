using System.Collections.Generic;
using System.Threading.Tasks;

using TravelAgencyAPI.Dto;

namespace TravelAgencyAPI.Services
{
    public interface ITravelService
    {
        Task<IEnumerable<TripDTO>> GetTripsAsync();
        Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId);
        Task<ClientResponseDTO> CreateClientAsync(ClientDTO clientDto);
        Task<bool> RegisterClientForTripAsync(int clientId, int tripId);
        Task<bool> UnregisterClientFromTripAsync(int clientId, int tripId);
    }
}