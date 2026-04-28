using System.ComponentModel.DataAnnotations;

namespace BHunter_Final.Models
{
    public class AddFavoriteMovie
    {
        [Required]
        public int MovieId { get; set; }

        public List<Movie> AvailableMovies { get; set; } = new();
    }
}
