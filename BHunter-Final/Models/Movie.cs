namespace BHunter_Final.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? StarRating { get; set; }
        public string? Maturity { get; set; }
        public int? GenreId { get; set; }
        public string? Runtime { get; set; }
        public int? ReleaseDate { get; set; }
        public string? Trailer { get; set; }
        public string? PosterImage { get; set; }

        public List<MovieCast> Cast { get; set; } = new();
        public ICollection<Favorites> Favorites { get; set; } = new List<Favorites>();
    }
}
