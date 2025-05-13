namespace TravelAgencyAPI.Models
{
    public class Country
    {
        public int IdCountry { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public ICollection<CountryTrip> CountryTrips { get; set; }
    }
}