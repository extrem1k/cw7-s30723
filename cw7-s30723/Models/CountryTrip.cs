namespace TravelAgencyAPI.Models
{
    public class CountryTrip
    {
        public int IdCountry { get; set; }
        public int IdTrip { get; set; }

        // Navigation properties
        public Country Country { get; set; }
        public Trip Trip { get; set; }
    }
}