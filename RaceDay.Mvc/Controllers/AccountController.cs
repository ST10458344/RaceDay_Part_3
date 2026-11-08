using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            try
            {
                var response = await client.PostAsJsonAsync(
                    "api/auth/register",
                    new
                    {
                        model.Email,
                        model.Password,
                        model.FirstName,
                        model.LastName,
                        model.PhoneNumber
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Account created successfully. Please log in.";

                    return RedirectToAction(nameof(Login));
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "This email is already registered.");
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        "Registration could not be completed.");
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

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                // Do not send an old API session during login.
                using var request = new HttpRequestMessage(
                    HttpMethod.Post, "api/auth/login")
                {
                    Content = JsonContent.Create(new
                    {
                        model.Email,
                        model.Password
                    })
                };

                using var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        "", "Invalid email or password.");
                    return View(model);
                }

                // Read the role returned by the API.
                using var json = JsonDocument.Parse(
                    await response.Content.ReadAsStringAsync());

                var role = json.RootElement
                    .GetProperty("role")
                    .GetString();

                if (role != "Participant" && role != "Organiser")
                {
                    ModelState.AddModelError(
                        "", "The account role is not supported.");
                    return View(model);
                }

                // Extract the API session cookie from this login response.
                if (!response.Headers.TryGetValues(
                        "Set-Cookie", out var cookieHeaders))
                {
                    ModelState.AddModelError(
                        "", "The API did not create a login session.");
                    return View(model);
                }

                var apiCookie = cookieHeaders
                    .Select(h => h.Split(';')[0].Trim())
                    .FirstOrDefault(c =>
                        c.StartsWith(
                            ".AspNetCore.Session=",
                            StringComparison.Ordinal));

                if (string.IsNullOrEmpty(apiCookie))
                {
                    ModelState.AddModelError(
                        "", "The API session cookie was missing.");
                    return View(model);
                }

                // Save the API cookie only in this user's MVC session.
                HttpContext.Session.SetString(
                    "ApiSessionCookie", apiCookie);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Email),
            new Claim(ClaimTypes.Role, role!)
        };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    });

                if (role == "Participant")
                {
                    return RedirectToAction("Index", "Participant");
                }


                if (role == "Participant")
                {
                    return RedirectToAction("Index", "Participant");
                }

                if (role == "Organiser")
                {
                    return RedirectToAction("Index", "Organiser");
                }

                return RedirectToAction("Index", "Home");

            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "", "Unable to connect to the RaceDay API.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var apiCookie = HttpContext.Session.GetString(
                "ApiSessionCookie");

            if (!string.IsNullOrEmpty(apiCookie))
            {
                try
                {
                    var client = _httpClientFactory.CreateClient(
                        "RaceDayApi");

                    using var request = new HttpRequestMessage(
                        HttpMethod.Post, "api/auth/logout");

                    request.Headers.TryAddWithoutValidation(
                        "Cookie", apiCookie);

                    using var response = await client.SendAsync(request);
                }
                catch (HttpRequestException)
                {
                    // Local MVC logout still proceeds if API is unavailable.
                }
            }

            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content("Access denied. You do not have permission.");
        }

    }
}

