using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class EditEventViewModel
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? RouteDescription { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxParticipants { get; set; }

        [Display(Name = "Publish Event")]
        public bool IsPublished { get; set; }
    }
}
