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
            var userRole = HttpContext.Session.GetString("UserRole");
            var token = HttpContext.Session.GetString("JWToken");

            bool hasValidRole = string.Equals(userRole, "Organiser", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(userRole, "Organizer", StringComparison.OrdinalIgnoreCase);

            return hasValidRole || (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")));
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
            if (int.TryParse(userIdString, out int userId) && userId > 0)
            {
                return userId;
            }
            return 1;
        }

        public async Task<IActionResult> Dashboard()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            int eventCount = 0;
            int upcomingCount = 0;
            string eventsJson = "[]";

            if (response.IsSuccessStatusCode)
            {
                eventsJson = await response.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(eventsJson);
                    var root = doc.RootElement;
                    
                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        eventCount = root.GetArrayLength();
                        
                        foreach (var element in root.EnumerateArray())
                        {
                            // Check for EventDate with casing variations
                            if ((element.TryGetProperty("eventDate", out var dateProp) || 
                                 element.TryGetProperty("EventDate", out dateProp)) &&
                                DateTime.TryParse(dateProp.GetString(), out DateTime evDate))
                            {
                                if (evDate > DateTime.Now)
                                {
                                    upcomingCount++;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Dashboard parsing error: {ex.Message}");
                }
            }

            ViewBag.EventCount = eventCount;
            ViewBag.UpcomingEvents = upcomingCount;
            ViewBag.TotalEnrolments = 0;
            ViewBag.EventsJson = eventsJson;

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
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string eventName, string description, DateTime eventDate, string location, string eventType, IFormFile bannerFile)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(eventName ?? ""), "EventName");
            form.Add(new StringContent(description ?? ""), "Description");
            form.Add(new StringContent(eventDate.ToString("o")), "EventDate");
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

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.GetAsync($"api/events/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            ViewBag.EventJson = content;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, string eventName, string description, DateTime eventDate, string location, string eventType, IFormFile bannerFile)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            var client = _apiService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(id.ToString()), "EventId");
            form.Add(new StringContent(eventName ?? ""), "EventName");
            form.Add(new StringContent(description ?? ""), "Description");
            form.Add(new StringContent(eventDate.ToString("o")), "EventDate");
            form.Add(new StringContent(location ?? ""), "Location");
            form.Add(new StringContent(eventType ?? "Run"), "EventType");
            form.Add(new StringContent(userId.ToString()), "OrganiserId");

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var fileStreamContent = new StreamContent(bannerFile.OpenReadStream());
                form.Add(fileStreamContent, "bannerFile", bannerFile.FileName);
            }

            var response = await client.PutAsync($"api/events/{id}", form);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Events");
            }

            ModelState.AddModelError(string.Empty, "Failed to update event.");
            return View();
        }

        // Allow both GET and POST so your delete link works instantly without form wrapping
        [HttpGet, HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            try
            {
                var client = _apiService.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"api/events/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Event deleted successfully.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "Event not found. It may have already been deleted.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "You do not have permission to delete this event.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to delete event: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the event: {ex.Message}";
            }

            return RedirectToAction("Events");
        }

        public async Task<IActionResult> Categories()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();

            // Fetch events so the dropdown populates
            var eventsResponse = await client.GetAsync("api/events");
            ViewBag.EventsJson = eventsResponse.IsSuccessStatusCode ? await eventsResponse.Content.ReadAsStringAsync() : "[]";

            // Fetch categories
            var catResponse = await client.GetAsync("api/categories");
            ViewBag.CategoriesJson = catResponse.IsSuccessStatusCode ? await catResponse.Content.ReadAsStringAsync() : "[]";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string categoryName, decimal distanceKm, decimal entryFee, int maxParticipants)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return RedirectToAction("Categories");

            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var categoryData = new
            {
                eventId,
                categoryName,
                distanceKm,
                entryFee,
                maxParticipants
            };

            var response = await client.PostAsJsonAsync("api/categories", categoryData);
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
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Fetch events for the event dropdown selector
            var eventsResponse = await client.GetAsync("api/events");
            ViewBag.EventsJson = eventsResponse.IsSuccessStatusCode
                ? await eventsResponse.Content.ReadAsStringAsync()
                : "[]";

            // Fetch enrolments for results entry
            var enrolResponse = await client.GetAsync("api/enrolments");
            ViewBag.EnrolmentsJson = enrolResponse.IsSuccessStatusCode
                ? await enrolResponse.Content.ReadAsStringAsync()
                : "[]";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitResult(int enrolmentId, int finishTimeSeconds, int finishingPosition)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return RedirectToAction("Results");

            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resultData = new
            {
                enrolmentId,
                finishTimeSeconds,
                finishingPosition
            };

            var response = await client.PostAsJsonAsync("api/results", resultData);
            return RedirectToAction("Results");
        }
    }
}

