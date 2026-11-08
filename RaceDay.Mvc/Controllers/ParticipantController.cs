using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using System.Net.Http.Json;

namespace RaceDay.Mvc.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Participant Dashboard
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: Participant/MyResults
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var apiCookie =
                HttpContext.Session.GetString("ApiSessionCookie");

            if (string.IsNullOrEmpty(apiCookie))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient("RaceDayApi");

                using var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/results/mine");

                request.Headers.TryAddWithoutValidation(
                    "Cookie", apiCookie);

                using var response = await client.SendAsync(request);

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage =
                        "Unable to retrieve your race results.";

                    return View(
                        new List<ParticipantResultViewModel>());
                }

                var results = await response.Content
                    .ReadFromJsonAsync<
                        List<ParticipantResultViewModel>>();

                return View(
                    results ?? new List<ParticipantResultViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "The RaceDay API is unavailable.";

                return View(
                    new List<ParticipantResultViewModel>());
            }
        }
    }
}
