namespace RaceDay.Mvc.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int? MaxEntries { get; set; }
    }
}
