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
            var role = HttpContext.Session.GetString("UserRole");
            var token = HttpContext.Session.GetString("JWToken");

            return !string.IsNullOrEmpty(token) &&
                   string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase);
        }

        private IActionResult? RedirectIfNotOrganiser()
        {
            if (!IsOrganiser())
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return RedirectToAction("Login", "Account", new { returnUrl });
            }
            return null;
        }

        private int GetUserId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            return int.TryParse(userIdString, out int userId) ? userId : 0;
        }

        public async Task<IActionResult> Dashboard()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var userId = GetUserId();
            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var events = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);
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
                        .ToList() ?? new List<JsonElement>();

                    ViewBag.EventCount = filteredEvents.Count;
                    ViewBag.TotalEnrolments = 0;
                    ViewBag.UpcomingEvents = filteredEvents.Count(e =>
                    {
                        if (e.TryGetProperty("eventDate", out var dateProp) ||
                            e.TryGetProperty("EventDate", out dateProp))
                        {
                            if (DateTime.TryParse(dateProp.GetString(), out DateTime eventDate))
                            {
                                return eventDate > DateTime.Now;
                            }
                        }
                        return false;
                    });

                    ViewBag.EventsJson = JsonSerializer.Serialize(filteredEvents);
                }
                catch
                {
                    ViewBag.EventCount = 0;
                    ViewBag.UpcomingEvents = 0;
                    ViewBag.TotalEnrolments = 0;
                }
            }

            return View();
        }

        public async Task<IActionResult> Events()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var userId = GetUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var events = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);
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

                    ViewBag.EventsJson = JsonSerializer.Serialize(filteredEvents ?? new List<JsonElement>());
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
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;
            return View();
        }

        // called by fetch() from the view, so it returns status codes instead of views
        [HttpPost]
        public async Task<IActionResult> CreateEvent(string eventName, string? description, DateTime date, string location, string eventType, IFormFile? bannerFile)
        {
            if (!IsOrganiser())
                return Unauthorized("Please log in as an organiser.");

            var client = _apiService.CreateClient();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(eventName ?? ""), "EventName");
            form.Add(new StringContent(description ?? ""), "Description");
            form.Add(new StringContent(date.ToString("o")), "EventDate");
            form.Add(new StringContent(location ?? ""), "Location");
            form.Add(new StringContent(eventType ?? "Run"), "EventType");

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var fileContent = new StreamContent(bannerFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(bannerFile.ContentType);
                form.Add(fileContent, "bannerFile", bannerFile.FileName);
            }

            var response = await client.PostAsync("api/events", form);
            if (response.IsSuccessStatusCode)
                return Ok(new { message = "Event created" });

            var error = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, $"API {(int)response.StatusCode}: {error}");
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.GetAsync($"api/events/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            ViewBag.EventJson = await response.Content.ReadAsStringAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, string title, string description, DateTime date, string location, string eventType, IFormFile? bannerFile)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(id.ToString()), "EventId");
            form.Add(new StringContent(title ?? ""), "EventName");
            form.Add(new StringContent(description ?? ""), "Description");
            form.Add(new StringContent(date.ToString("o")), "EventDate");
            form.Add(new StringContent(location ?? ""), "Location");
            form.Add(new StringContent(eventType ?? "Run"), "EventType");

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var fileContent = new StreamContent(bannerFile.OpenReadStream());
                form.Add(fileContent, "bannerFile", bannerFile.FileName);
            }

            var response = await client.PutAsync($"api/events/{id}", form);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Events");
            }

            ModelState.AddModelError(string.Empty, "Failed to update event.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            await client.DeleteAsync($"api/events/{id}");
            return RedirectToAction("Events");
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

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string categoryName, decimal distanceKm, decimal entryFee, int maxParticipants)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();

            var categoryData = new
            {
                eventId,
                categoryName,
                distanceKm,
                entryFee,
                maxParticipants
            };

            await client.PostAsJsonAsync("api/categories", categoryData);
            return RedirectToAction("Categories");
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

        [HttpPost]
        public async Task<IActionResult> SubmitResult(int enrolmentId, int finishTimeSeconds, int finishingPosition)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();

            var resultData = new
            {
                enrolmentId,
                finishTimeSeconds,
                finishingPosition
            };

            await client.PostAsJsonAsync("api/results", resultData);
            return RedirectToAction("Results");
        }
    }
}

