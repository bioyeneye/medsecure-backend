using System.ComponentModel.DataAnnotations;

namespace MedSecureSystem.Application.Features.Businesses.Commands
{
    public class CreateBusinessCommand
    {
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Country { get; set; }
    }
}

