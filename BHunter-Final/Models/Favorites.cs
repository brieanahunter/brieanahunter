using System.ComponentModel.DataAnnotations;

namespace BHunter_Final.Models
{
    public class Favorites
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }

        public User User { get; set; }
        public Movie Movie { get; set; }

    }
}
