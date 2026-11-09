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
            return int.TryParse(HttpContext.Session.GetString("UserId"), out int id) ? id : 0;
        }

        private static string? ReadMessage(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString();
            }
            catch { }
            return null;
        }

        private async Task<string> GetJson(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "[]";
        }

        // only this organiser's events
        private async Task<string> GetMyEventsJson(HttpClient client)
        {
            var userId = GetUserId();
            var json = await GetJson(client, "api/events");
            try
            {
                var events = JsonSerializer.Deserialize<List<JsonElement>>(json) ?? new List<JsonElement>();
                var mine = events.Where(e =>
                    (e.TryGetProperty("organiserId", out var p) || e.TryGetProperty("OrganiserId", out p))
                    && p.GetInt32() == userId).ToList();
                return JsonSerializer.Serialize(mine);
            }
            catch
            {
                return "[]";
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            ViewBag.EventsJson = await GetMyEventsJson(client);
            ViewBag.EnrolmentsJson = await GetJson(client, "api/enrolments");
            return View();
        }

        public async Task<IActionResult> Events()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            ViewBag.EventsJson = await GetMyEventsJson(client);
            return View();
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;
            return View();
        }

        // called by fetch() from the view, so it returns status codes
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
            if (!response.IsSuccessStatusCode) return NotFound();

            ViewBag.EventId = id;
            ViewBag.EventJson = await response.Content.ReadAsStringAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, string title, string? description, DateTime date, string location, string eventType, IFormFile? bannerFile)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(title ?? ""), "EventName");
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

            var response = await client.PutAsync($"api/events/{id}", form);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event updated.";
                return RedirectToAction("Events");
            }

            var body = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = ReadMessage(body) ?? $"Could not update event (API {(int)response.StatusCode}).";
            return RedirectToAction("EditEvent", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.DeleteAsync($"api/events/{id}");

            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Event deleted.";
            else
                TempData["ErrorMessage"] = "Could not delete that event.";

            return RedirectToAction("Events");
        }

        public async Task<IActionResult> Categories()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            ViewBag.EventsJson = await GetMyEventsJson(client);
            ViewBag.CategoriesJson = await GetJson(client, "api/categories");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string categoryName, decimal distanceKm, decimal entryFee, int maxParticipants)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.PostAsJsonAsync("api/categories",
                new { eventId, categoryName, distanceKm, entryFee, maxParticipants });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Category added.";
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = ReadMessage(body) ?? $"Could not add category (API {(int)response.StatusCode}).";
            }

            return RedirectToAction("Categories");
        }

        public async Task<IActionResult> Enrolments()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            ViewBag.EventsJson = await GetMyEventsJson(client);
            ViewBag.EnrolmentsJson = await GetJson(client, "api/enrolments");
            return View();
        }

        public async Task<IActionResult> Results()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            ViewBag.EventsJson = await GetMyEventsJson(client);
            ViewBag.EnrolmentsJson = await GetJson(client, "api/enrolments");
            ViewBag.ResultsJson = await GetJson(client, "api/results");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitResult(int enrolmentId, string finishTimeStr, int finishingPosition)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            // expects HH:MM:SS
            var parts = (finishTimeStr ?? "").Split(':');
            if (parts.Length != 3
                || !int.TryParse(parts[0], out int h)
                || !int.TryParse(parts[1], out int m)
                || !int.TryParse(parts[2], out int s))
            {
                TempData["ErrorMessage"] = "Finish time must look like 00:45:30 (HH:MM:SS).";
                return RedirectToAction("Results");
            }

            int finishTimeSeconds = h * 3600 + m * 60 + s;

            var client = _apiService.CreateClient();
            var response = await client.PostAsJsonAsync("api/results",
                new { enrolmentId, finishTimeSeconds, finishingPosition });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Result saved.";
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = ReadMessage(body) ?? $"Could not save result (API {(int)response.StatusCode}).";
            }

            return RedirectToAction("Results");
        }
    }
}

