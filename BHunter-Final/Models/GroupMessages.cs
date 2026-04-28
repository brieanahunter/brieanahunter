using System.ComponentModel.DataAnnotations.Schema;

namespace BHunter_Final.Models
{
    [Table("Groups")]
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }

        public string? Description { get; set; }

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public int? MovieOfTheWeekId { get; set; }
        public Movie? MovieOfTheWeek { get; set; }

        public DateTime? WeekStartDate { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Messages> Messages { get; set; } = new List<Messages>();


    }

    public class Messages
    {
        public int MessageId { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string MessageText { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
