using System.ComponentModel.DataAnnotations;

namespace BHunter_Final.Models
{
    public class ResetPassword
    {
        public string Email { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
