using Microsoft.AspNetCore.Mvc;
using RaceDay_EventMS.Web.Models;
using RaceDay_EventMS.Web.Services;
using System.Text.Json;

namespace RaceDay_EventMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _apiService.CreateClient();
            var response = await client.PostAsJsonAsync("api/auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var token = root.GetProperty("token").GetString();
                var role = root.GetProperty("role").GetString();
                var userId = root.GetProperty("userId").GetInt32();

                HttpContext.Session.SetString("JWToken", token ?? "");
                HttpContext.Session.SetString("UserRole", role ?? "");
                HttpContext.Session.SetString("UserId", userId.ToString());
                HttpContext.Session.SetString("UserEmail", model.Email);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Check your credentials.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _apiService.CreateClient();
            var response = await client.PostAsJsonAsync("api/auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, "Registration failed. Email might already be in use.");
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}