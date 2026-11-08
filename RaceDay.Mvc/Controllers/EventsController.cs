using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using System.Net.Http.Json;

namespace RaceDay.Mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            IHttpClientFactory httpClientFactory,
            ILogger<EventsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                var events = await client.GetFromJsonAsync<List<EventViewModel>>(
                    "api/events");

                // Only display published, upcoming events.
                var upcomingEvents = (events ?? new List<EventViewModel>())
                    .Where(e => e.IsPublished &&
                                e.EventDate.Date >= DateTime.Today)
                    .OrderBy(e => e.EventDate)
                    .ToList();

                return View(upcomingEvents);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve RaceDay events.");
                ViewBag.ErrorMessage =
                    "Events are temporarily unavailable. Please try again later.";

                return View(new List<EventViewModel>());
            }
        }
    }
}
