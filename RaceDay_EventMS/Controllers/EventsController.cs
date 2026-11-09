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

        // GET: api/Events (public)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            return await _context.Events
                .Include(e => e.Categories)
                .ToListAsync();
        }

        // GET: api/Events/5 (public)
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound();
            }

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
                {
                    return BadRequest(new { message = "Event name, location and type are required." });
                }

                if (!DateTime.TryParse(EventDate, out DateTime eventDate))
                {
                    return BadRequest(new { message = "Invalid event date format." });
                }

                // organiser comes from the token, not the form
                var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(idClaim, out int organiserId))
                {
                    return Unauthorized(new { message = "Could not read user id from token." });
                }

                var bannerUrl = string.Empty;

                if (bannerFile != null && bannerFile.Length > 0)
                {
                    var result = await UploadBannerFile(bannerFile);
                    if (result.ContainsKey("imageUrl"))
                    {
                        bannerUrl = result["imageUrl"].ToString() ?? string.Empty;
                    }
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

        // PUT: api/Events/5 (organisers only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] EventUpdateDto dto)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return NotFound(new { message = "Event not found." });
                }

                // Verify the user is the organiser of this event
                var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(idClaim, out int organiserId) || eventItem.OrganiserId != organiserId)
                {
                    return Forbid();
                }

                // Update fields if provided
                if (!string.IsNullOrWhiteSpace(dto.EventName))
                    eventItem.EventName = dto.EventName;

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    eventItem.Description = dto.Description;

                if (dto.EventDate != default)
                    eventItem.EventDate = dto.EventDate;

                if (!string.IsNullOrWhiteSpace(dto.Location))
                    eventItem.Location = dto.Location;

                if (!string.IsNullOrWhiteSpace(dto.EventType))
                    eventItem.EventType = dto.EventType;

                // Handle optional banner update if a new file is uploaded
                if (dto.BannerFile != null && dto.BannerFile.Length > 0)
                {
                    var result = await UploadBannerFile(dto.BannerFile);
                    if (result.ContainsKey("imageUrl"))
                    {
                        eventItem.BannerUrl = result["imageUrl"].ToString() ?? string.Empty;
                    }
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Failed to update event", error = detail });
            }
        }

        // DELETE: api/Events/5 (organisers only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return NotFound(new { message = "Event not found." });
                }

                // Verify the user is the organiser of this event
                var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(idClaim, out int organiserId) || eventItem.OrganiserId != organiserId)
                {
                    return Forbid();
                }

                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "Failed to delete event", error = detail });
            }
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