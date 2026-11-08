using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    [Authorize(Roles = "Participant")]
    public class EnrolmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrolmentsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var apiCookie = HttpContext.Session
                .GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get, "api/enrolments/mine");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                using var response =
                    await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    return RedirectToAction("AccessDenied", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error =
                        "Unable to load your enrolments.";
                    return View();
                }

                ViewBag.EnrolmentsJson =
                    await response.Content.ReadAsStringAsync();

                return View();
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "Cannot connect to the RaceDay API.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int categoryId, string? emergencyContact)
        {
            if (categoryId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a valid race category.";
                return RedirectToAction("Index", "Events");
            }

            var apiCookie = HttpContext.Session
                .GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory
                    .CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Post, "api/enrolments");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                request.Content = JsonContent.Create(new
                {
                    CategoryId = categoryId,
                    EmergencyContact = emergencyContact
                });

                using var response =
                    await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "You have successfully enrolled in the race!";

                    return RedirectToAction("Index");
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    return RedirectToAction("AccessDenied", "Account");

                string message = "Race registration failed.";

                try
                {
                    using var json = JsonDocument.Parse(
                        await response.Content.ReadAsStringAsync());

                    if (json.RootElement.TryGetProperty(
                        "message", out var error))
                    {
                        message = error.GetString() ?? message;
                    }
                }
                catch (JsonException)
                {
                    // Use the default error message.
                }

                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index", "Events");
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Index", "Events");
            }
        }
    }
}
