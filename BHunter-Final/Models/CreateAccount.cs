using System.ComponentModel.DataAnnotations;

namespace BHunter_Final.Models
{
    public class CreateAccount
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImagePath { get; set; }

        public int? GroupId { get; set; }
    }
}