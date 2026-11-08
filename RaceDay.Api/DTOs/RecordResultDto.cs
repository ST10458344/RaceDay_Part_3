using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class RecordResultDto
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan? FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int? PositionOverall { get; set; }

        [Range(1, int.MaxValue)]
        public int? PositionCategory { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
