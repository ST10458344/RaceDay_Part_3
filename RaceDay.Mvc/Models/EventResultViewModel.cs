namespace RaceDay.Mvc.Models
{
    public class EventResultViewModel
    {
        public int EnrolmentId { get; set; }
        public string? BibNumber { get; set; }
        public string Status { get; set; } = "";
        public string ParticipantName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public RecordedResultViewModel? Result { get; set; }
    }

    public class RecordedResultViewModel
    {
        public int ResultId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int? PositionOverall { get; set; }
        public int? PositionCategory { get; set; }
        public string? Notes { get; set; }
    }

    public class ManageResultsViewModel
    {
        public int EventId { get; set; }

        public List<EventResultViewModel> Enrolments { get; set; }
            = new();
    }
}
