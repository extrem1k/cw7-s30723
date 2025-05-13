using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TravelAgencyAPI.Dto;
using TravelAgencyAPI.Models;

namespace TravelAgencyAPI.Services
{
    public class TravelService : ITravelService
    {
        private readonly string _connectionString;

        public TravelService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<TripDTO>> GetTripsAsync()
        {
            var trips = new List<TripDTO>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

              
                var query = @"
                    SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, 
                           c.IdCountry, c.Name AS CountryName
                    FROM Trip t
                    JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                    JOIN Country c ON ct.IdCountry = c.IdCountry
                    ORDER BY t.IdTrip";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int currentTripId = -1;
                        TripDTO currentTrip = null;

                        while (await reader.ReadAsync())
                        {
                            var tripId = reader.GetInt32(reader.GetOrdinal("IdTrip"));

                            if (tripId != currentTripId)
                            {
                                currentTripId = tripId;
                                currentTrip = new TripDTO
                                {
                                    IdTrip = tripId,
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Description = !reader.IsDBNull(reader.GetOrdinal("Description")) 
                                        ? reader.GetString(reader.GetOrdinal("Description")) 
                                        : null,
                                    DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                                    DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                                    MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                                    Countries = new List<string>()
                                };
                                trips.Add(currentTrip);
                            }
                            
                            currentTrip.Countries.Add(reader.GetString(reader.GetOrdinal("CountryName")));
                        }
                    }
                }
            }

            return trips;
        }

        public async Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId)
        {
            var clientTrips = new List<ClientTripDTO>();

            
            if (!await ClientExistsAsync(clientId))
            {
                return null; 
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT t.IdTrip, t.Name, t.DateFrom, t.DateTo, ct.RegisteredAt, ct.PaymentDate
                    FROM Client_Trip ct
                    JOIN Trip t ON ct.IdTrip = t.IdTrip
                    WHERE ct.IdClient = @ClientId
                    ORDER BY t.DateFrom";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            clientTrips.Add(new ClientTripDTO
                            {
                                IdTrip = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                                TripName = reader.GetString(reader.GetOrdinal("Name")),
                                DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                                DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                                RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                                PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) 
                                    ? null 
                                    : (int?)reader.GetInt32(reader.GetOrdinal("PaymentDate"))
                            });
                        }
                    }
                }
            }

            return clientTrips;
        }

        public async Task<ClientResponseDTO> CreateClientAsync(ClientDTO clientDto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                
                var checkQuery = "SELECT COUNT(*) FROM Client WHERE Email = @Email";
                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Email", clientDto.Email);
                    var count = (int)await checkCommand.ExecuteScalarAsync();
                    if (count > 0)
                    {
                        throw new InvalidOperationException("A client with this email already exists");
                    }
                }

                var query = @"
                    INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                    OUTPUT INSERTED.IdClient
                    VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
                    command.Parameters.AddWithValue("@LastName", clientDto.LastName);
                    command.Parameters.AddWithValue("@Email", clientDto.Email);
                    command.Parameters.AddWithValue("@Telephone", (object)clientDto.Telephone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Pesel", (object)clientDto.Pesel ?? DBNull.Value);

                    var idClient = (int)await command.ExecuteScalarAsync();
                    
                    return new ClientResponseDTO
                    {
                        IdClient = idClient,
                        FirstName = clientDto.FirstName,
                        LastName = clientDto.LastName,
                        Email = clientDto.Email,
                        Telephone = clientDto.Telephone,
                        Pesel = clientDto.Pesel
                    };
                }
            }
        }

        public async Task<bool> RegisterClientForTripAsync(int clientId, int tripId)
        {
          
            if (!await ClientExistsAsync(clientId))
            {
                throw new InvalidOperationException("Client does not exist");
            }

           
            var tripDetails = await GetTripDetailsAsync(tripId);
            if (tripDetails == null)
            {
                throw new InvalidOperationException("Trip does not exist");
            }

           
            if (await IsClientRegisteredForTripAsync(clientId, tripId))
            {
                throw new InvalidOperationException("Client is already registered for this trip");
            }

          
            int currentRegisteredCount = await GetTripRegistrationCountAsync(tripId);
            if (currentRegisteredCount >= tripDetails.MaxPeople)
            {
                throw new InvalidOperationException("Trip is already full");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                    VALUES (@IdClient, @IdTrip, @RegisteredAt, NULL)";

                using (var command = new SqlCommand(query, connection))
                {
                   
                    int registeredAt = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

                    command.Parameters.AddWithValue("@IdClient", clientId);
                    command.Parameters.AddWithValue("@IdTrip", tripId);
                    command.Parameters.AddWithValue("@RegisteredAt", registeredAt);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task<bool> UnregisterClientFromTripAsync(int clientId, int tripId)
        {
          
            if (!await IsClientRegisteredForTripAsync(clientId, tripId))
            {
                throw new InvalidOperationException("Client is not registered for this trip");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    DELETE FROM Client_Trip
                    WHERE IdClient = @IdClient AND IdTrip = @IdTrip";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdClient", clientId);
                    command.Parameters.AddWithValue("@IdTrip", tripId);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

    
        private async Task<bool> ClientExistsAsync(int clientId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT COUNT(*) FROM Client WHERE IdClient = @ClientId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);
                    var count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        private async Task<TripDetailsDTO> GetTripDetailsAsync(int tripId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople
                    FROM Trip
                    WHERE IdTrip = @TripId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TripId", tripId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new TripDetailsDTO
                            {
                                IdTrip = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Description = !reader.IsDBNull(reader.GetOrdinal("Description")) 
                                    ? reader.GetString(reader.GetOrdinal("Description")) 
                                    : null,
                                DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                                DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                                MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople"))
                            };
                        }
                        return null;
                    }
                }
            }
        }

        private async Task<int> GetTripRegistrationCountAsync(int tripId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TripId", tripId);
                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }

        private async Task<bool> IsClientRegisteredForTripAsync(int clientId, int tripId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT COUNT(*) 
                    FROM Client_Trip 
                    WHERE IdClient = @ClientId AND IdTrip = @TripId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);
                    command.Parameters.AddWithValue("@TripId", tripId);
                    var count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
    }
}
