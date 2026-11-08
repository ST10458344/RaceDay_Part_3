using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context) => _context = context;


        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.StartTime,
                    e.Location,
                    e.RouteDescription,
                    e.MaxParticipants,
                    e.IsPublished,

                    Categories = e.Categories.Select(c => new
                    {
                        c.CategoryId,
                        c.EventId,
                        c.Name,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MinAge,
                        c.MaxAge,
                        c.MaxEntries
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
        }


        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventDto dto)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(role)) return Unauthorized("User not authenticated.");
            if (role != "Organiser") return StatusCode(403, "Forbidden - Organiser role required.");

            var newEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                Location = dto.Location,
                RouteDescription = dto.RouteDescription,
                MaxParticipants = dto.MaxParticipants,
                IsPublished = dto.IsPublished,
                CreatedByUserId = userId!.Value
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvents), new { id = newEvent.EventId }, newEvent);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Organiser access required."
                });
            }

            var events = await _context.Events
                .AsNoTracking()
                .Where(e => e.CreatedByUserId == userId.Value)
                .OrderByDescending(e => e.EventDate)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.StartTime,
                    e.Location,
                    e.MaxParticipants,
                    e.IsPublished,
                    e.RouteDescription,
                    Categories = e.Categories.Select(c => new
                    {
                        c.CategoryId,
                        c.Name,
                        c.DistanceKm,
                        c.EntryFee,
                        c.MaxEntries
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/events/1/categories
        [HttpPost("{eventId:int}/categories")]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            [FromBody] CreateCategoryDto dto)
        {
            // Verify the API login session.
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Organiser access required."
                });
            }

            // Find the event and verify ownership.
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (raceEvent.CreatedByUserId != userId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only manage your own events."
                });
            }

            // Validate the category information.
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            if (dto.MinAge.HasValue &&
                dto.MaxAge.HasValue &&
                dto.MinAge.Value > dto.MaxAge.Value)
            {
                return BadRequest(new
                {
                    message = "Minimum age cannot exceed maximum age."
                });
            }

            // Prevent duplicate category names within the event.
            var categoryName = dto.Name.Trim();

            var categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.EventId == eventId &&
                    c.Name == categoryName);

            if (categoryExists)
            {
                return Conflict(new
                {
                    message = "This category already exists for this event."
                });
            }

            // Create the new race category.
            var category = new Category
            {
                EventId = eventId,
                Name = categoryName,
                DistanceKm = dto.DistanceKm,
                EntryFee = dto.EntryFee,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                MaxEntries = dto.MaxEntries
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Return a simple response without navigation properties.
            return StatusCode(201, new
            {
                message = "Race category created successfully.",
                category.CategoryId,
                category.EventId,
                category.Name,
                category.DistanceKm,
                category.EntryFee,
                category.MinAge,
                category.MaxAge,
                category.MaxEntries
            });
        }

        // PUT: api/events/1
        // Allows an Organiser to edit an event they own.
        [HttpPut("{eventId:int}")]
        public async Task<IActionResult> UpdateEvent(
            int eventId,
            [FromBody] CreateEventDto dto)
        {
            // Check the logged-in user.
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return Unauthorized(new
                {
                    message = "Please log in."
                });
            }

            if (role != "Organiser")
            {
                return StatusCode(403, new
                {
                    message = "Organiser access required."
                });
            }

            // Find the event.
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Only the event creator can edit it.
            if (raceEvent.CreatedByUserId != userId.Value)
            {
                return StatusCode(403, new
                {
                    message = "You can only edit your own events."
                });
            }

            // Validate the updated details.
            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Location))
            {
                return BadRequest(new
                {
                    message = "Event name and location are required."
                });
            }

            if (dto.EventDate.Date < DateTime.Today)
            {
                return BadRequest(new
                {
                    message = "The event date cannot be in the past."
                });
            }

            if (dto.MaxParticipants < 1)
            {
                return BadRequest(new
                {
                    message = "Maximum participants must be greater than zero."
                });
            }

            // Update the event details.
            raceEvent.Name = dto.Name.Trim();
            raceEvent.Description = dto.Description;
            raceEvent.EventDate = dto.EventDate;
            raceEvent.StartTime = dto.StartTime;
            raceEvent.Location = dto.Location.Trim();
            raceEvent.RouteDescription = dto.RouteDescription;
            raceEvent.MaxParticipants = dto.MaxParticipants;
            raceEvent.IsPublished = dto.IsPublished;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Race event updated successfully.",
                raceEvent.EventId,
                raceEvent.Name,
                raceEvent.EventDate,
                raceEvent.Location,
                raceEvent.IsPublished
            });
        }



    }
}