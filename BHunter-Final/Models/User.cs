using System.ComponentModel.DataAnnotations.Schema;

namespace BHunter_Final.Models
{
    [Table("Users")]
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string? ProfileImagePath { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public ICollection<Favorites> Favorites { get; set; } = new List<Favorites>();
        public ICollection<Messages> Messages { get; set; } = new List<Messages>();
    }
}
