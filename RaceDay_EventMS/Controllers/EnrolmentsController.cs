using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_EventMS.Data;
using RaceDay_EventMS.Models;
using System.Security.Claims;

namespace RaceDay_EventMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var userId) ? userId : 0;
        }

        // GET: api/Enrolments (organisers)
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolments()
        {
            var now = DateTime.Now;
            var list = await _context.Enrolments
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentId,
                    participantId = e.ParticipantId,
                    participantName = e.Participant!.Email,
                    categoryId = e.CategoryId,
                    categoryName = e.Category!.CategoryName,
                    eventId = e.Category!.EventId,
                    eventName = e.Category!.Event!.EventName,
                    enrolmentDate = e.EnrolmentDate,
                    status = e.Result != null ? "Completed"
                           : (e.Category!.Event!.EventDate < now ? "Awaiting Results" : "Confirmed")
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Enrolments/my (logged-in participant)
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Could not read user id from token." });

            var now = DateTime.Now;
            var list = await _context.Enrolments
                .Where(e => e.ParticipantId == userId)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentId,
                    categoryName = e.Category!.CategoryName,
                    distanceKm = e.Category!.DistanceKm,
                    entryFee = e.Category!.EntryFee,
                    eventId = e.Category!.EventId,
                    eventName = e.Category!.Event!.EventName,
                    eventDate = e.Category!.Event!.EventDate,
                    location = e.Category!.Event!.Location,
                    enrolmentDate = e.EnrolmentDate,
                    status = e.Result != null ? "Completed"
                           : (e.Category!.Event!.EventDate < now ? "Awaiting Results" : "Confirmed")
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Enrolments/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .Where(e => e.EnrolmentId == id)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentId,
                    participantId = e.ParticipantId,
                    categoryId = e.CategoryId,
                    categoryName = e.Category!.CategoryName,
                    eventId = e.Category!.EventId,
                    enrolmentDate = e.EnrolmentDate
                })
                .FirstOrDefaultAsync();

            if (enrolment == null) return NotFound();

            if (!User.IsInRole("Organiser") && enrolment.participantId != GetUserId())
                return Forbid();

            return Ok(enrolment);
        }

        // POST: api/Enrolments, body: { "categoryId": 3 }
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> PostEnrolment([FromBody] EnrolDto dto)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Could not read user id from token." });

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category == null)
                return BadRequest(new { message = "The specified category does not exist." });

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == userId && e.Category!.EventId == category.EventId);

            if (alreadyEnrolled)
                return BadRequest(new { message = "You are already enrolled in this event." });

            var currentCount = await _context.Enrolments
                .CountAsync(e => e.CategoryId == category.CategoryId);

            if (currentCount >= category.MaxParticipants)
                return BadRequest(new { message = $"This category is full (max {category.MaxParticipants} participants)." });

            var enrolment = new Enrolment
            {
                ParticipantId = userId,
                CategoryId = category.CategoryId,
                EnrolmentDate = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrolment),
                new { id = enrolment.EnrolmentId },
                new { enrolmentId = enrolment.EnrolmentId, categoryId = enrolment.CategoryId });
        }

        // DELETE: api/Enrolments/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound();

            if (!User.IsInRole("Organiser") && enrolment.ParticipantId != GetUserId())
                return Forbid();

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class EnrolDto
    {
        public int CategoryId { get; set; }
    }
}