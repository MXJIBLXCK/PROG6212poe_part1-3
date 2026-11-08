using Microsoft.AspNetCore.Mvc;
using RaceDay_EventMS.Web.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RaceDay_EventMS.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _apiService;

        public OrganiserController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        private IActionResult? RedirectIfNotOrganiser()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");
            return null;
        }

        public async Task<IActionResult> Events()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
                return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var events = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);

                    // Filter to only include events created by this organizer
                    var filteredEvents = events?
                        .Where(e =>
                        {
                            if (e.TryGetProperty("organiserId", out var idProp) ||
                                e.TryGetProperty("OrganiserId", out idProp))
                            {
                                return idProp.GetInt32() == userId;
                            }
                            return false;
                        })
                        .ToList();

                    var filteredJson = JsonSerializer.Serialize(filteredEvents ?? new List<JsonElement>());
                    ViewBag.EventsJson = filteredJson;
                }
                catch
                {
                    ViewBag.EventsJson = "[]";
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string title, string description, DateTime date, string location, string eventType, IFormFile bannerFile)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
                return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(title ?? ""), "Title");
            form.Add(new StringContent(description ?? ""), "Description");
            form.Add(new StringContent(date.ToString("o")), "EventDate");
            form.Add(new StringContent(location ?? ""), "Location");
            form.Add(new StringContent(eventType ?? "Run"), "EventType");
            form.Add(new StringContent(userId.ToString()), "OrganiserId");

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var fileStreamContent = new StreamContent(bannerFile.OpenReadStream());
                form.Add(fileStreamContent, "bannerFile", bannerFile.FileName);
            }

            var response = await client.PostAsync("api/events", form);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Events");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"Failed to create event: {errorContent}");
            return View();
        }

        public IActionResult Dashboard()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            // Optionally fetch summary stats here
            return View();
        }

        public async Task<IActionResult> Categories()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/categories");

            if (response.IsSuccessStatusCode)
                ViewBag.CategoriesJson = await response.Content.ReadAsStringAsync();

            return View();
        }

        public async Task<IActionResult> Enrolments()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/enrolments");

            if (response.IsSuccessStatusCode)
                ViewBag.EnrolmentsJson = await response.Content.ReadAsStringAsync();

            return View();
        }

        public async Task<IActionResult> Results()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
                ViewBag.EventsJson = await response.Content.ReadAsStringAsync();

            return View();
        }
    }
}

