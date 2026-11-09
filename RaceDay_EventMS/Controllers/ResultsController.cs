using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_EventMS.Data;
using RaceDay_EventMS.Models;

namespace RaceDay_EventMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results (any logged-in user)
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .Select(r => new
                {
                    enrolmentId = r.EnrolmentId,
                    participantName = r.Enrolment!.Participant!.Email,
                    categoryName = r.Enrolment!.Category!.CategoryName,
                    eventId = r.Enrolment!.Category!.EventId,
                    eventName = r.Enrolment!.Category!.Event!.EventName,
                    finishTimeSeconds = r.FinishTimeSeconds,
                    finishingPosition = r.FinishingPosition
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/5 (by enrolment id)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .Where(r => r.EnrolmentId == id)
                .Select(r => new
                {
                    enrolmentId = r.EnrolmentId,
                    participantName = r.Enrolment!.Participant!.Email,
                    categoryName = r.Enrolment!.Category!.CategoryName,
                    eventId = r.Enrolment!.Category!.EventId,
                    finishTimeSeconds = r.FinishTimeSeconds,
                    finishingPosition = r.FinishingPosition
                })
                .FirstOrDefaultAsync();

            if (result == null) return NotFound();
            return Ok(result);
        }

        // GET: api/Results/my (participant's own race history)
        [HttpGet("my")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int userId);

            var list = await _context.Results
                .Where(r => r.Enrolment!.ParticipantId == userId)
                .Select(r => new
                {
                    eventName = r.Enrolment!.Category!.Event!.EventName,
                    eventDate = r.Enrolment!.Category!.Event!.EventDate,
                    categoryName = r.Enrolment!.Category!.CategoryName,
                    finishTimeSeconds = r.FinishTimeSeconds,
                    finishingPosition = r.FinishingPosition,
                    totalFinishers = _context.Results.Count(x => x.Enrolment!.CategoryId == r.Enrolment!.CategoryId)
                })
                .OrderByDescending(x => x.eventDate)
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/Results (organisers only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PostResult([FromBody] ResultDto dto)
        {
            if (!await _context.Enrolments.AnyAsync(e => e.EnrolmentId == dto.EnrolmentId))
                return BadRequest(new { message = "That enrolment does not exist." });

            if (await _context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
                return BadRequest(new { message = "A result was already captured for this participant." });

            if (dto.FinishTimeSeconds <= 0 || dto.FinishingPosition <= 0)
                return BadRequest(new { message = "Finish time and position must be greater than zero." });

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTimeSeconds = dto.FinishTimeSeconds,
                FinishingPosition = dto.FinishingPosition
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResult), new { id = result.EnrolmentId },
                new { enrolmentId = result.EnrolmentId });
        }
    }

    public class ResultDto
    {
        public int EnrolmentId { get; set; }
        public int FinishTimeSeconds { get; set; }
        public int FinishingPosition { get; set; }
    }
}