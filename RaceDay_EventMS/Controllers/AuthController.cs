using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay_EventMS.Data;
using RaceDay_EventMS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDay_EventMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
            var key = Encoding.UTF8.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
    }
}