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

        private IActionResult RedirectIfNotOrganiser()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");
            return null;
        }

        public async Task<IActionResult> Events()
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
                ViewBag.EventsJson = await response.Content.ReadAsStringAsync();

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
        public async Task<IActionResult> CreateEvent(string title, string description, DateTime date, IFormFile bannerFile)
        {
            var redirectResult = RedirectIfNotOrganiser();
            if (redirectResult != null) return redirectResult;

            var client = _apiService.CreateClient();
            string bannerUrl = string.Empty;

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

