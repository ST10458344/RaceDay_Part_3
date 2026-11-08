namespace RaceDay.Api.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? RouteDescription { get; set; }
        public int? MaxParticipants { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPublished { get; set; } = false;

        public User? CreatedByUser { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}