using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAgencyAPI.Dto;
using TravelAgencyAPI.Services;

namespace TravelAgencyAPI.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ITravelService _travelService;

        public ClientsController(ITravelService travelService)
        {
            _travelService = travelService;
        }

       
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientDTO clientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _travelService.CreateClientAsync(clientDto);
                return CreatedAtAction(nameof(GetClientTrips), new { id = result.IdClient }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // In production, you would log this exception
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while creating the client", details = ex.Message });
            }
        }

      
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            try
            {
                var clientTrips = await _travelService.GetClientTripsAsync(id);
                
                if (clientTrips == null)
                {
                    return NotFound(new { message = $"Client with ID {id} not found" });
                }

                if (clientTrips.Count() == 0)
                {
                    return Ok(new { message = $"Client with ID {id} has no registered trips", trips = clientTrips });
                }

                return Ok(clientTrips);
            }
            catch (Exception ex)
            {
                
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving client trips", details = ex.Message });
            }
        }

       
        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            try
            {
                var result = await _travelService.RegisterClientForTripAsync(id, tripId);
                return Ok(new { message = $"Client {id} successfully registered for trip {tripId}" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
               
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while registering client for trip", details = ex.Message });
            }
        }

      
        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> UnregisterClientFromTrip(int id, int tripId)
        {
            try
            {
                var result = await _travelService.UnregisterClientFromTripAsync(id, tripId);
                return Ok(new { message = $"Client {id} successfully unregistered from trip {tripId}" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while unregistering client from trip", details = ex.Message });
            }
        }
    }
}
