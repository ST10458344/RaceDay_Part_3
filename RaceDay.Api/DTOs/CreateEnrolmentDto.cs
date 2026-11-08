using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateEnrolmentDto
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a valid race category.")]
        public int CategoryId { get; set; }

        [StringLength(150)]
        public string? EmergencyContact { get; set; }
    }
}
