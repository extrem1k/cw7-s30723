using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TravelAgencyAPI.Dto;
using TravelAgencyAPI.Services;

namespace TravelAgencyAPI.Controllers
{
    [Route("api/trips")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITravelService _travelService;

        public TripsController(ITravelService travelService)
        {
            _travelService = travelService ?? throw new ArgumentNullException(nameof(travelService));
        }

        /// <summary>
        /// Gets all available trips with their country information
        /// </summary>
        /// <returns>List of all trips with details</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTrips()
        {
            try
            {
                var trips = await _travelService.GetTripsAsync();
                return Ok(trips);
            }
            catch (Exception ex)
            {
                // In production, you would log this exception
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while retrieving trips", details = ex.Message });
            }
        }
    }
}