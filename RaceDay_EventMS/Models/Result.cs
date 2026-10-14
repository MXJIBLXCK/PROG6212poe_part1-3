using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_EventMS.Models
{
    public class Result
    {
        [Key, ForeignKey("Enrolment")]
        public int EnrolmentId { get; set; }

        public Enrolment? Enrolment { get; set; }

        [Required]
        public int FinishTimeSeconds { get; set; }

        [Required]
        public int FinishingPosition { get; set; }
    }
}
