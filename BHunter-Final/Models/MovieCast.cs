namespace BHunter_Final.Models
{
    public class MovieCast
    {
        public int CastId { get; set; }
        public int MovieId { get; set; }
        public string ActorName { get; set; }
        public string CharacterName { get; set; }

        public Movie Movie { get; set; }
    }
}
