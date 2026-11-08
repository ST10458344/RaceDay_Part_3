using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class CreateEventViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Event Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; } =
            DateTime.Today.AddDays(7);

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; } =
            new TimeSpan(7, 0, 0);

        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Route Description")]
        public string? RouteDescription { get; set; }

        [Range(1, 100000)]
        [Display(Name = "Maximum Participants")]
        public int? MaxParticipants { get; set; }

        [Display(Name = "Publish Event")]
        public bool IsPublished { get; set; } = false;
    }
}
