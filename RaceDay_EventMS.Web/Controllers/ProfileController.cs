using Microsoft.AspNetCore.Mvc;
using RaceDay_EventMS.Web.Services;

namespace RaceDay_EventMS.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApiService _apiService;

        public ProfileController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            var client = _apiService.CreateClient();
            var response = await client.GetAsync("api/auth/profile");

            if (response.IsSuccessStatusCode)
            {
                ViewBag.ProfileJson = await response.Content.ReadAsStringAsync();
            }

            return View();
        }
    }
}
