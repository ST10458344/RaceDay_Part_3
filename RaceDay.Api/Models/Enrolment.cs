namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
        public string? BibNumber { get; set; }
        public string Status { get; set; } = "Confirmed";
        public string? EmergencyContact { get; set; }

        public User? User { get; set; }
        public Category? Category { get; set; }
        public Result? Result { get; set; }
    }
}