namespace TravelAgencyAPI.Dto
{
    public class ClientTripDTO
    {
        public int IdTrip { get; set; }
        public string TripName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int RegisteredAt { get; set; }
        public int? PaymentDate { get; set; }
        public object Trip { get; set; }
    }
}