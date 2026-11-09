using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay_EventMS.Data;
using RaceDay_EventMS.Models;
using System.Security.Claims;

namespace RaceDay_EventMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;

        public EventsController(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private int GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var userId) ? userId : 0;
        }

        // GET: api/Events (public)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events
                .Include(e => e.Categories)
                .ToListAsync();
        }

        // GET: api/Events/5 (public)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null) return NotFound();
            return eventItem;
        }

        // POST: api/Events (organisers only)
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<ActionResult<Event>> PostEvent(
            [FromForm] string EventName,
            [FromForm] string? Description,
            [FromForm] string EventDate,
            [FromForm] string Location,
            [FromForm] string EventType,
            [FromForm] IFormFile? bannerFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EventName) || string.IsNullOrWhiteSpace(Location) || string.IsNullOrWhiteSpace(EventType))
                    return BadRequest(new { message = "Event name, location and type are required." });

                if (!DateTime.TryParse(EventDate, out DateTime eventDate))
                    return BadRequest(new { message = "Invalid event date format." });

                var organiserId = GetUserId();
                if (organiserId == 0)
                    return Unauthorized(new { message = "Could not read user id from token." });

                var bannerUrl = string.Empty;
                if (bannerFile != null && bannerFile.Length > 0)
                {
                    var upload = await UploadBannerFile(bannerFile);
                    if (upload.ContainsKey("imageUrl"))
                        bannerUrl = upload["imageUrl"].ToString() ?? string.Empty;
                }

                var newEvent = new Event
                {
                    EventName = EventName,
                    Description = Description ?? string.Empty,
                    EventDate = eventDate,
                    Location = Location,
                    EventType = EventType,
                    OrganiserId = organiserId,
                    BannerUrl = bannerUrl
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventId }, newEvent);
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Failed to create event", error = detail });
            }
        }

        // PUT: api/Events/5 (organiser, own events only)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> PutEvent(
            int id,
            [FromForm] string EventName,
            [FromForm] string? Description,
            [FromForm] string EventDate,
            [FromForm] string Location,
            [FromForm] string EventType,
            [FromForm] IFormFile? bannerFile)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != GetUserId()) return Forbid();

            if (!DateTime.TryParse(EventDate, out DateTime eventDate))
                return BadRequest(new { message = "Invalid event date format." });

            ev.EventName = EventName;
            ev.Description = Description ?? string.Empty;
            ev.EventDate = eventDate;
            ev.Location = Location;
            ev.EventType = EventType;

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var upload = await UploadBannerFile(bannerFile);
                if (upload.ContainsKey("imageUrl"))
                    ev.BannerUrl = upload["imageUrl"].ToString();
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Events/5 (organiser, own events only)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != GetUserId()) return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<Dictionary<string, object>> UploadBannerFile(IFormFile file)
        {
            var result = new Dictionary<string, object>();

            if (file == null || file.Length == 0)
                return result;

            try
            {
                var connectionString = _configuration["AzureBlobStorage:ConnectionString"];
                var containerName = _configuration["AzureBlobStorage:ContainerName"] ?? "event-banners";

                if (!string.IsNullOrEmpty(connectionString) && connectionString != "your_azure_connection_string_here")
                {
                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var blobClient = containerClient.GetBlobClient(fileName);

                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    result["imageUrl"] = blobClient.Uri.ToString();
                }
                else
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    result["imageUrl"] = $"{baseUrl}/uploads/{uniqueFileName}";
                }
            }
            catch (Exception ex)
            {
                result["error"] = ex.Message;
            }

            return result;
        }
    }
}