using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_EventMS.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty; // e.g., "10km", "21km", "Open"

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKm { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal EntryFee { get; set; }

        [Required]
        public int MaxParticipants { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}