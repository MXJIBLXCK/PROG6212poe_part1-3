using Microsoft.AspNetCore.Mvc;
using RaceDay_EventMS.Web.Services;

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
            return role == "Participant";
        }

        public async Task<IActionResult> Events()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/events");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.EventsJson = content;
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int eventId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var response = await client.PostAsync($"api/enrolments?eventId={eventId}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Successfully enrolled in the event!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to enrol. You may already be enrolled for this event.";
            }

            return RedirectToAction(nameof(Events));
        }

        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/enrolments/my");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.EnrolmentsJson = content;
            }

            return View();
        }
    }
}
