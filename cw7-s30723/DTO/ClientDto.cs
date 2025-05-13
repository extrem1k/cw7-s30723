using System.ComponentModel.DataAnnotations;

namespace TravelAgencyAPI.Dto
{
    public class ClientDTO
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(120, ErrorMessage = "First name cannot exceed 120 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(120, ErrorMessage = "Last name cannot exceed 120 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(120, ErrorMessage = "Email cannot exceed 120 characters")]
        public string Email { get; set; }

        [MaxLength(120, ErrorMessage = "Telephone cannot exceed 120 characters")]
        public string Telephone { get; set; }

        [MaxLength(120, ErrorMessage = "Pesel cannot exceed 120 characters")]
        public string Pesel { get; set; }
    }
    
    public class ClientResponseDTO : ClientDTO
    {
        public int IdClient { get; set; }
    }
}