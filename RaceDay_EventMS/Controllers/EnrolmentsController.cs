using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_EventMS.Data;
using RaceDay_EventMS.Models;

namespace RaceDay_EventMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrolment>>> GetEnrolments()
        {
            return await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .ToListAsync();
        }

        // GET: api/Enrolments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrolment>> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound();
            }

            return enrolment;
        }

        // POST: api/Enrolments
        [HttpPost]
        public async Task<ActionResult<Enrolment>> PostEnrolment(Enrolment enrolment)
        {
            // 1. Find the Category to check max participants and get its details
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == enrolment.CategoryId);

            if (category == null)
            {
                return BadRequest(new { message = "The specified category does not exist." });
            }

            // 2. Count current enrolments for this category
            var currentEnrolmentCount = await _context.Enrolments
                .CountAsync(e => e.CategoryId == enrolment.CategoryId);

            // 3. Validate participant limit
            if (currentEnrolmentCount >= category.MaxParticipants)
            {
                return BadRequest(new { message = $"Registration failed. This category has reached its maximum capacity of {category.MaxParticipants} participants." });
            }

            // 4. If under limit, proceed with saving the enrolment
            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnrolment), new { id = enrolment.EnrolmentId }, enrolment);
        }

        // DELETE: api/Enrolments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrolment(int id)
        {
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null)
            {
                return NotFound();
            }

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
