using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/results")]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/results/event/1
        // Lists enrolments and recorded results for an Organiser's event.
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
                return Unauthorized(new { message = "Please log in." });

            if (role != "Organiser")
                return StatusCode(403, new
                {
                    message = "Organiser access required."
                });

            var raceEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (raceEvent == null)
                return NotFound(new { message = "Event not found." });

            if (raceEvent.CreatedByUserId != userId.Value)
                return StatusCode(403, new
                {
                    message = "You can only view results for your own events."
                });

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.Category != null &&
                            e.Category.EventId == eventId)
                .OrderBy(e => e.Category!.Name)
                .ThenBy(e => e.EnrolmentId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.BibNumber,
                    e.Status,

                    ParticipantName = e.User == null
                        ? "Unknown Participant"
                        : e.User.FirstName + " " + e.User.LastName,

                    CategoryName = e.Category!.Name,

                    Result = e.Result == null
                        ? null
                        : new
                        {
                            e.Result.ResultId,
                            e.Result.FinishTime,
                            e.Result.PositionOverall,
                            e.Result.PositionCategory,
                            e.Result.Notes
                        }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/results
        // Records a participant's result.
        [HttpPost]
        public async Task<IActionResult> RecordResult(
            [FromBody] RecordResultDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
                return Unauthorized(new { message = "Please log in." });

            if (role != "Organiser")
                return StatusCode(403, new
                {
                    message = "Organiser access required."
                });

            if (dto.EnrolmentId <= 0)
                return BadRequest(new
                {
                    message = "A valid enrolment is required."
                });

            if (!dto.FinishTime.HasValue ||
                dto.FinishTime.Value <= TimeSpan.Zero)
                return BadRequest(new
                {
                    message = "Finish time must be greater than zero."
                });

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == dto.EnrolmentId);

            if (enrolment == null)
                return NotFound(new
                {
                    message = "Enrolment not found."
                });

            if (enrolment.Category?.Event == null)
                return BadRequest(new
                {
                    message = "The enrolment has no valid event."
                });

            if (enrolment.Category.Event.CreatedByUserId != userId.Value)
                return StatusCode(403, new
                {
                    message = "You can only record results for your own events."
                });

            if (enrolment.Status != "Confirmed")
                return BadRequest(new
                {
                    message = "Only confirmed enrolments can receive results."
                });

            var resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentId == dto.EnrolmentId);

            if (resultExists)
                return Conflict(new
                {
                    message = "A result has already been recorded for this enrolment."
                });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime.Value,
                PositionOverall = dto.PositionOverall,
                PositionCategory = dto.PositionCategory,
                RecordedByUserId = userId.Value,
                RecordedAt = DateTime.UtcNow,
                Notes = dto.Notes
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Race result recorded successfully.",
                result.ResultId,
                result.EnrolmentId,
                result.FinishTime,
                result.PositionOverall,
                result.PositionCategory
            });
        }

        // GET: api/results/mine
        // Returns race results belonging to the logged-in participant.
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
                return Unauthorized(new
                {
                    message = "Please log in."
                });

            if (role != "Participant")
                return StatusCode(403, new
                {
                    message = "Participant access required."
                });

            var results = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.UserId == userId.Value)
                .OrderByDescending(e => e.Category!.Event!.EventDate)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.BibNumber,
                    e.Status,

                    EventName = e.Category!.Event!.Name,
                    EventDate = e.Category.Event.EventDate,
                    CategoryName = e.Category.Name,

                    Result = e.Result == null
                        ? null
                        : new
                        {
                            e.Result.ResultId,
                            e.Result.FinishTime,
                            e.Result.PositionOverall,
                            e.Result.PositionCategory,
                            e.Result.RecordedAt,
                            e.Result.Notes
                        }
                })
                .ToListAsync();

            return Ok(results);
        }

    }
}
