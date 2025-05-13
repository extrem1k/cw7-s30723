﻿namespace TravelAgencyAPI.Models
{
    public class ClientTrip
    {
        public int IdClient { get; set; }
        public int IdTrip { get; set; }
        public int RegisteredAt { get; set; }
        public int? PaymentDate { get; set; }

        // Navigation properties
        public Client Client { get; set; }
        public Trip Trip { get; set; }
    }
}