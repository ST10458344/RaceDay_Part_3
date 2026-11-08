using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class RecordResultViewModel
    {
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        [Required]
        public string FinishTime { get; set; } = "";

        [Range(1, int.MaxValue)]
        public int? PositionOverall { get; set; }

        [Range(1, int.MaxValue)]
        public int? PositionCategory { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
