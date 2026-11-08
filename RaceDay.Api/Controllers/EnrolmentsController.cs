using Microsoft.Data.SqlClient;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnrolmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/enrolments/mine
        [HttpGet("mine")]
        public async Task<IActionResult> MyEnrolments()
        {
            // Read the identity from the API session.
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in first."
                });
            }

            if (role != "Participant")
            {
                return StatusCode(403, new
                {
                    message = "Only Participants can view this page."
                });
            }

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.UserId == userId.Value)
                .OrderByDescending(e => e.EnrolmentDate)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.EnrolmentDate,
                    e.Status,
                    e.BibNumber,
                    CategoryName = e.Category!.Name,
                    DistanceKm = e.Category.DistanceKm,
                    EventName = e.Category.Event!.Name,
                    EventDate = e.Category.Event.EventDate,
                    Location = e.Category.Event.Location
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentDto dto)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
                return Unauthorized(new { message = "Please log in first." });

            if (role != "Participant")
                return StatusCode(403, new
                {
                    message = "Only Participants can enrol in races."
                });

            // Verify that the account is still active and has the role.
            bool validParticipant = await _context.Users
                .AsNoTracking()
                .AnyAsync(u =>
                    u.UserId == userId.Value &&
                    u.IsActive &&
                    u.UserRoles.Any(ur =>
                        ur.Role!.RoleName == "Participant"));

            if (!validParticipant)
                return StatusCode(403, new
                {
                    message = "Your account is not authorised to enrol."
                });

            // A serializable transaction prevents concurrent registrations
            // from bypassing the capacity checks.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                var category = await _context.Categories
                    .Include(c => c.Event)
                    .FirstOrDefaultAsync(c =>
                        c.CategoryId == dto.CategoryId);

                if (category == null || category.Event == null)
                    return NotFound(new
                    {
                        message = "Race category not found."
                    });

                var raceEvent = category.Event;

                if (!raceEvent.IsPublished)
                    return BadRequest(new
                    {
                        message = "This event is not published."
                    });

                if (raceEvent.EventDate.Date < DateTime.Today)
                    return BadRequest(new
                    {
                        message = "This event has already taken place."
                    });

                bool alreadyEnrolled = await _context.Enrolments
                    .AnyAsync(e =>
                        e.UserId == userId.Value &&
                        e.CategoryId == category.CategoryId);

                if (alreadyEnrolled)
                    return Conflict(new
                    {
                        message = "You are already enrolled in this category."
                    });

                int categoryEntries = await _context.Enrolments
                    .CountAsync(e =>
                        e.CategoryId == category.CategoryId);

                if (category.MaxEntries.HasValue &&
                    categoryEntries >= category.MaxEntries.Value)
                    return Conflict(new
                    {
                        message = "This race category is full."
                    });

                int eventEntries = await _context.Enrolments
                    .CountAsync(e =>
                        e.Category!.EventId == raceEvent.EventId);

                if (raceEvent.MaxParticipants.HasValue &&
                    eventEntries >= raceEvent.MaxParticipants.Value)
                    return Conflict(new
                    {
                        message = "This event is full."
                    });

                var enrolment = new Enrolment
                {
                    UserId = userId.Value,
                    CategoryId = category.CategoryId,
                    EnrolmentDate = DateTime.UtcNow,
                    Status = "Confirmed",
                    EmergencyContact = dto.EmergencyContact
                };

                _context.Enrolments.Add(enrolment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(201, new
                {
                    message = "Race enrolment successful.",
                    enrolmentId = enrolment.EnrolmentId
                });
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sql &&
                      (sql.Number == 2601 || sql.Number == 2627))
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this category."
                });
            }
        }

    }
}
