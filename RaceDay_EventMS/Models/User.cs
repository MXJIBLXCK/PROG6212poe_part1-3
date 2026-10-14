using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_EventMS.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"

        // Navigation property for 1-to-1 Profile
        public UserProfile? UserProfile { get; set; }

        // Navigation properties for relationships
        public ICollection<Event> EventsOrganised { get; set; } = new List<Event>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}