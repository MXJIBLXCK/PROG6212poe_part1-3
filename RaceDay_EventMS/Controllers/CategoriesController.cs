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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return category;
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PostCategory(Category category)
        {
            var ev = await _context.Events.FindAsync(category.EventId);
            if (ev == null)
                return BadRequest(new { message = "That event does not exist." });

            // organisers can only add categories to their own events
            if (ev.OrganiserId != GetUserId())
                return Forbid();

            category.CategoryId = 0;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId },
                new { categoryId = category.CategoryId, categoryName = category.CategoryName });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.CategoryId) return BadRequest();

            var existing = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (existing == null) return NotFound();
            if (existing.Event?.OrganiserId != GetUserId()) return Forbid();

            existing.CategoryName = category.CategoryName;
            existing.DistanceKm = category.DistanceKm;
            existing.EntryFee = category.EntryFee;
            existing.MaxParticipants = category.MaxParticipants;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();
            if (category.Event?.OrganiserId != GetUserId()) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
