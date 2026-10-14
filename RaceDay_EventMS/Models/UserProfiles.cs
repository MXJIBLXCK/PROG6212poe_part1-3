using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_EventMS.Models
{
    public class UserProfile
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(100)]
        public string? RunningClub { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(20)]
        public string? EmergencyContactPhone { get; set; }

        public User? User { get; set; }
    }
}