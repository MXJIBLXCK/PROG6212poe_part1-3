using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_EventMS.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string EventType { get; set; } = string.Empty; // "Run", "Walk", "Cycle"

        [Required]
        public int OrganiserId { get; set; }

        [ForeignKey("OrganiserId")]
        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}