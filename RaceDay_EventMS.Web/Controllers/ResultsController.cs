using Microsoft.AspNetCore.Mvc;
using RaceDay_EventMS.Web.Services;

namespace RaceDay_EventMS.Web.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ApiService _apiService;

        public ResultsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Leaderboard(int eventId)
        {
            var client = _apiService.CreateClient();
            var response = await client.GetAsync($"api/results/event/{eventId}");

            if (response.IsSuccessStatusCode)
            {
                ViewBag.LeaderboardJson = await response.Content.ReadAsStringAsync();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnterResult(int eventId, int participantId, string finishTime, int rank)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Organiser") return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var resultDto = new { EventId = eventId, ParticipantId = participantId, FinishTime = finishTime, Rank = rank };

            var response = await client.PostAsJsonAsync("api/results", resultDto);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Race result recorded successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to record result.";
            }

            return RedirectToAction("Events", "Organiser");
        }
    }
}
