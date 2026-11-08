using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class CreateCategoryViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "100000")]
        [Display(Name = "Distance (km)")]
        public decimal DistanceKm { get; set; }

        [Range(typeof(decimal), "0", "1000000")]
        [Display(Name = "Entry Fee (R)")]
        public decimal EntryFee { get; set; }

        [Range(0, 120)]
        [Display(Name = "Minimum Age")]
        public int? MinAge { get; set; }

        [Range(0, 120)]
        [Display(Name = "Maximum Age")]
        public int? MaxAge { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "Maximum Entries")]
        public int? MaxEntries { get; set; }
    }
}
