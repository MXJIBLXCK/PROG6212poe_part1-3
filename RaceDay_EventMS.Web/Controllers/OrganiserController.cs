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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string title, string description, DateTime date, IFormFile bannerFile)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(title ?? ""), "Title");
            form.Add(new StringContent(description ?? ""), "Description");
            form.Add(new StringContent(date.ToString("o")), "Date");

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var fileStreamContent = new StreamContent(bannerFile.OpenReadStream());
                // Corrected: passing 3 arguments instead of 4
                form.Add(fileStreamContent, "bannerFile", bannerFile.FileName);
            }

            var response = await client.PostAsync("api/events", form);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Events");
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

