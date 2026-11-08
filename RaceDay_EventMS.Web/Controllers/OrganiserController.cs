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
            return role == "Organiser";
        }

        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Pass event list to view (we'll create the view next)
                ViewBag.EventsJson = content;
            }

            return View();
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string title, string description, DateTime date, IFormFile bannerFile)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            string bannerUrl = string.Empty;

            // 1. Upload banner file to API Azure Blob endpoint if provided
            if (bannerFile != null && bannerFile.Length > 0)
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = bannerFile.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(bannerFile.ContentType);
                content.Add(fileContent, "file", bannerFile.FileName);

                var uploadResponse = await client.PostAsync("api/events/upload-banner", content);
                if (uploadResponse.IsSuccessStatusCode)
                {
                    var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(uploadResult);
                    bannerUrl = doc.RootElement.GetProperty("imageUrl").GetString() ?? "";
                }
            }

            // 2. Submit event payload to API
            var eventDto = new { Title = title, Description = description, Date = date, BannerUrl = bannerUrl };
            var createResponse = await client.PostAsJsonAsync("api/events", eventDto);

            if (createResponse.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Events));
            }

            ModelState.AddModelError(string.Empty, "Failed to create event via API.");
            return View();
        }
    }
}
