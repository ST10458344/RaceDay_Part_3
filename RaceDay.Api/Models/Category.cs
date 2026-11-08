namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public decimal EntryFee { get; set; } = 0;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int? MaxEntries { get; set; }

        public Event? Event { get; set; }
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}