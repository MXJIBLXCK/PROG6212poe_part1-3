using System.ComponentModel.DataAnnotations;

namespace RaceDay_EventMS.Models
{
    public class EventUpdateDto
    {
        [MaxLength(150)]
        public string? EventName { get; set; }

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? EventType { get; set; }

        public IFormFile? BannerFile { get; set; }
    }
}