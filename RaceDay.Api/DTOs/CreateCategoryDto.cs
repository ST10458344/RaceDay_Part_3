using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "100000")]
        public decimal DistanceKm { get; set; }

        [Range(typeof(decimal), "0", "1000000")]
        public decimal EntryFee { get; set; }

        [Range(0, 120)]
        public int? MinAge { get; set; }

        [Range(0, 120)]
        public int? MaxAge { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxEntries { get; set; }
    }
}
