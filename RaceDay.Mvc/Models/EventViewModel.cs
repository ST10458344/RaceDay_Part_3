namespace RaceDay.Mvc.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? RouteDescription { get; set; }
        public int? MaxParticipants { get; set; }
        public bool IsPublished { get; set; }

        public List<CategoryViewModel> Categories { get; set; }
            = new List<CategoryViewModel>();
    }
}
