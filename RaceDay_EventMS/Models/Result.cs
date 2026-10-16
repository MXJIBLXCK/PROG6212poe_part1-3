using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_EventMS.Models;

/// <summary>
/// Represents the result of a participant's enrolment in a category, including finish time and position.
/// </summary>
public class Result
{
    [Key, ForeignKey(nameof(Enrolment))]
    public int EnrolmentId { get; set; }

    public Enrolment? Enrolment { get; set; }

    [Required]
    public int FinishTimeSeconds { get; set; }

    [Required]
    public int FinishingPosition { get; set; }
}
