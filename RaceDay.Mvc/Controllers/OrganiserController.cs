using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using System.Net.Http.Json;

namespace RaceDay.Mvc.Controllers
{
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Events()
        {
            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
            {
                TempData["ErrorMessage"] =
                    "Your API session has expired. Please log in again.";

                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get, "api/events/mine");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                using var response = await client.SendAsync(request);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    TempData["ErrorMessage"] =
                        "Your session has expired. Please log in again.";

                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage =
                        "Unable to retrieve your events.";

                    return View(new List<EventViewModel>());
                }

                var events = await response.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                return View(events ?? new List<EventViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "The RaceDay API is unavailable.";

                return View(new List<EventViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new CreateEventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(CreateEventViewModel model)
        {
            if (model.EventDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.EventDate),
                    "The event date cannot be in the past.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var apiCookie = HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
            {
                ModelState.AddModelError(
                    "",
                    "Your API session has expired. Please log in again.");

                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/events");

                request.Headers.TryAddWithoutValidation(
                    "Cookie",
                    apiCookie);

                request.Content = JsonContent.Create(new
                {
                    model.Name,
                    model.Description,
                    model.EventDate,
                    model.StartTime,
                    model.Location,
                    model.RouteDescription,
                    model.MaxParticipants,
                    model.IsPublished
                });

                using var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Race event created successfully.";

                    return RedirectToAction(nameof(Events));
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(
                        "",
                        "Your API session has expired. Please log in again.");
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.Forbidden)
                {
                    ModelState.AddModelError(
                        "",
                        "You do not have permission to create events.");
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to create the event. Please try again.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "The RaceDay API is unavailable.");
            }

            return View(model);
        }

        // Display the Add Race Category form.
        [HttpGet]
        public async Task<IActionResult> CreateCategory(int eventId)
        {
            if (eventId <= 0)
            {
                return BadRequest("Invalid event ID.");
            }

            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
            {
                TempData["ErrorMessage"] =
                    "Your API session has expired. Please log in again.";

                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get, "api/events/mine");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                using var response = await client.SendAsync(request);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to verify event ownership.";

                    return RedirectToAction(nameof(Events));
                }

                var events = await response.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                if (events == null ||
                    !events.Any(e => e.EventId == eventId))
                {
                    return NotFound(
                        "This event does not exist or does not belong to you.");
                }

                return View(new CreateCategoryViewModel
                {
                    EventId = eventId
                });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The RaceDay API is unavailable.";

                return RedirectToAction(nameof(Events));
            }
        }

        // Submit a new race category to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            CreateCategoryViewModel model)
        {
            if (model.EventId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.EventId),
                    "Invalid event ID.");
            }

            if (model.MinAge.HasValue &&
                model.MaxAge.HasValue &&
                model.MinAge.Value > model.MaxAge.Value)
            {
                ModelState.AddModelError(
                    nameof(model.MaxAge),
                    "Maximum age cannot be lower than minimum age.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
            {
                ModelState.AddModelError(
                    "",
                    "Your API session has expired. Please log in again.");

                return View(model);
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/events/{model.EventId}/categories");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                request.Content = JsonContent.Create(new
                {
                    model.Name,
                    model.DistanceKm,
                    model.EntryFee,
                    model.MinAge,
                    model.MaxAge,
                    model.MaxEntries
                });

                using var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Race category created successfully.";

                    return RedirectToAction(nameof(Events));
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(
                        "",
                        "Your API session has expired. Please log in again.");
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.Forbidden)
                {
                    ModelState.AddModelError(
                        "",
                        "You do not have permission to manage this event.");
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError(
                        "",
                        "The selected event could not be found.");
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        nameof(model.Name),
                        "This category already exists for the event.");
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to create the category. Check the details and try again.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "The RaceDay API is unavailable.");
            }

            return View(model);
        }

        // GET: Organiser/EditEvent?eventId=1
        [HttpGet]
        public async Task<IActionResult> EditEvent(int eventId)
        {
            if (eventId <= 0)
                return BadRequest("Invalid event ID.");

            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
                return RedirectToAction("Login", "Account");

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get, "api/events/mine");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                using var response = await client.SendAsync(request);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to load the event.";

                    return RedirectToAction(nameof(Events));
                }

                var events = await response.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                var raceEvent = events?
                    .FirstOrDefault(e => e.EventId == eventId);

                if (raceEvent == null)
                    return NotFound("Event not found.");

                var model = new EditEventViewModel
                {
                    EventId = raceEvent.EventId,
                    Name = raceEvent.Name,
                    Description = raceEvent.Description,
                    EventDate = raceEvent.EventDate,
                    StartTime = raceEvent.StartTime,
                    Location = raceEvent.Location,
                    RouteDescription = raceEvent.RouteDescription,
                    MaxParticipants = raceEvent.MaxParticipants ?? 0,
                    IsPublished = raceEvent.IsPublished
                };

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The RaceDay API is unavailable.";

                return RedirectToAction(nameof(Events));
            }
        }

        // POST: Organiser/EditEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(
            EditEventViewModel model)
        {
            if (model.EventId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.EventId),
                    "Invalid event ID.");
            }

            if (model.EventDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.EventDate),
                    "Event date cannot be in the past.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
            {
                ModelState.AddModelError(
                    "",
                    "Your session has expired. Please log in again.");

                return View(model);
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"api/events/{model.EventId}");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                request.Content = JsonContent.Create(new
                {
                    model.Name,
                    model.Description,
                    model.EventDate,
                    model.StartTime,
                    model.Location,
                    model.RouteDescription,
                    model.MaxParticipants,
                    model.IsPublished
                });

                using var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Race event updated successfully.";

                    return RedirectToAction(nameof(Events));
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError(
                        "", "Your session has expired.");
                }
                else if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    ModelState.AddModelError(
                        "", "You cannot edit this event.");
                }
                else
                {
                    ModelState.AddModelError(
                        "", "Unable to update the event.");
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "", "The RaceDay API is unavailable.");
            }

            return View(model);
        }

        // GET: Organiser/ManageResults?eventId=1
        [HttpGet]
        public async Task<IActionResult> ManageResults(int eventId)
        {
            if (eventId <= 0)
                return BadRequest("Invalid event ID.");

            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
                return RedirectToAction("Login", "Account");

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"api/results/event/{eventId}");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                using var response = await client.SendAsync(request);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Event not found.");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to load race results.";

                    return RedirectToAction(nameof(Events));
                }

                var enrolments = await response.Content
                    .ReadFromJsonAsync<List<EventResultViewModel>>();

                return View(new ManageResultsViewModel
                {
                    EventId = eventId,
                    Enrolments = enrolments ?? new()
                });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The RaceDay API is unavailable.";

                return RedirectToAction(nameof(Events));
            }
        }

        // POST: Organiser/RecordResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(
            RecordResultViewModel model)
        {
            if (!TimeSpan.TryParseExact(
                    model.FinishTime,
                    @"hh\:mm\:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var finishTime) ||
                finishTime <= TimeSpan.Zero)
            {
                ModelState.AddModelError(
                    nameof(model.FinishTime),
                    "Enter a valid finish time in HH:mm:ss format.");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Invalid result details. Use HH:mm:ss for finish time.";

                return RedirectToAction(
                    nameof(ManageResults),
                    new { eventId = model.EventId });
            }

            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
                return RedirectToAction("Login", "Account");

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Post, "api/results");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                request.Content = JsonContent.Create(new
                {
                    model.EnrolmentId,
                    FinishTime = finishTime,
                    model.PositionOverall,
                    model.PositionCategory,
                    model.Notes
                });

                using var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Race result recorded successfully.";
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.Conflict)
                {
                    TempData["ErrorMessage"] =
                        "A result already exists for this participant.";
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] =
                        "You cannot record results for this event.";
                }
                else if (response.StatusCode ==
                         System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Unable to record the result. Check the details.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The RaceDay API is unavailable.";
            }

            return RedirectToAction(
                nameof(ManageResults),
                new { eventId = model.EventId });
        }


    }
}
