namespace BHunter_Final.Models
{
    public class UpcomingReleases
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MaturityRating { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Trailer { get; set; }

        public string PosterImage { get; set; }

        public int GenreId { get; set; }

        public bool IsHidden { get; set; } = false;
    }
}
