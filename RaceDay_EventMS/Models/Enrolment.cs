using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_EventMS.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [ForeignKey("ParticipantId")]
        public User? Participant { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        public Result? Result { get; set; }
    }
}