using Microsoft.AspNetCore.Mvc;
using RaceDay_EventMS.Web.Services;
using System.Text.Json;

namespace RaceDay_EventMS.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _apiService;

        public ParticipantController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private bool IsParticipant()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase);
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

        private async Task<string> GetJson(string url)
        {
            var client = _apiService.CreateClient();
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "[]";
        }

        public async Task<IActionResult> Events()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            ViewBag.EventsJson = await GetJson("api/events");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int categoryId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            if (categoryId <= 0)
            {
                TempData["ErrorMessage"] = "Please pick a category before enrolling.";
                return RedirectToAction(nameof(Events));
            }

            var client = _apiService.CreateClient();
            var response = await client.PostAsJsonAsync("api/enrolments", new { categoryId });

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "You're enrolled!";
                return RedirectToAction(nameof(MyEnrolments));
            }

            var body = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = ReadMessage(body) ?? $"Failed to enrol (API {(int)response.StatusCode}).";
            return RedirectToAction(nameof(Events));
        }

        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            ViewBag.EnrolmentsJson = await GetJson("api/enrolments/my");
            return View();
        }

        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");
            ViewBag.ResultsJson = await GetJson("api/results/my");
            return View();
        }
    }
}