using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_EventMS.Data;
using RaceDay_EventMS.Models;

namespace RaceDay_EventMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication for all result endpoints
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results (Authenticated users can view all results)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            return await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                        .ThenInclude(c => c!.Event)
                .ToListAsync();
        }

        // GET: api/Results/5 (View result by EnrolmentId)
        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(int id)
        {
            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                        .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(r => r.EnrolmentId == id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        // POST: api/Results (Only Organisers can capture finish times and positions)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Result>> PostResult(Result result)
        {
            // Verify that the enrolment exists
            var enrolmentExists = await _context.Enrolments.AnyAsync(e => e.EnrolmentId == result.EnrolmentId);
            if (!enrolmentExists)
            {
                return BadRequest(new { message = "Associated Enrolment ID does not exist." });
            }

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResult), new { id = result.EnrolmentId }, result);
        }
    }
}