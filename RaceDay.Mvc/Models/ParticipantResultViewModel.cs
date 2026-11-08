namespace RaceDay.Mvc.Models
{
    public class ParticipantResultViewModel
    {
        public int EnrolmentId { get; set; }

        public string? BibNumber { get; set; }

        public string Status { get; set; } = "";

        public string EventName { get; set; } = "";

        public DateTime EventDate { get; set; }

        public string CategoryName { get; set; } = "";

        public ParticipantRecordedResultViewModel? Result
        {
            get;
            set;
        }
    }

    public class ParticipantRecordedResultViewModel
    {
        public int ResultId { get; set; }

        public TimeSpan FinishTime { get; set; }

        public int? PositionOverall { get; set; }

        public int? PositionCategory { get; set; }

        public DateTime RecordedAt { get; set; }

        public string? Notes { get; set; }
    }
}
