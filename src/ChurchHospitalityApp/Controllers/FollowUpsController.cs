using System.Security.Claims;
using ChurchHospitalityApp.Data;
using ChurchHospitalityApp.DTOs;
using ChurchHospitalityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchHospitalityApp.Controllers
{
    [ApiController]
    [Route("api/followups")]
    [Authorize]
    public class FollowUpsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public FollowUpsController(ApplicationDbContext db)
        {
            _db = db;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private bool IsAdmin => User.IsInRole(AppRoles.Admin);

        /// <summary>
        /// scope=mine (default): follow-ups assigned to the caller.
        /// scope=all (Admin): every follow-up, optionally filtered by assignedToUserId.
        /// scope=unassigned (Admin): follow-ups awaiting assignment.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FollowUpDto>>> GetAll(
            [FromQuery] string scope = "mine",
            [FromQuery] string? status = null,
            [FromQuery] string? assignedToUserId = null)
        {
            var query = _db.FollowUps
                .Include(f => f.Guest)
                .Include(f => f.AssignedToUser)
                .AsQueryable();

            if (scope == "unassigned")
            {
                if (!IsAdmin) return Forbid();
                query = query.Where(f => f.AssignedToUserId == null);
            }
            else if (scope == "all")
            {
                if (!IsAdmin) return Forbid();
                if (!string.IsNullOrWhiteSpace(assignedToUserId))
                {
                    query = query.Where(f => f.AssignedToUserId == assignedToUserId);
                }
            }
            else
            {
                query = query.Where(f => f.AssignedToUserId == CurrentUserId);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FollowUpStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(f => f.Status == parsedStatus);
            }

            var followUps = await query.OrderBy(f => f.ScheduledFor).ToListAsync();
            return Ok(followUps.Select(FollowUpDto.From).ToList());
        }

        [HttpPut("{id}/assign")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Assign(int id, [FromBody] AssignFollowUpRequest request)
        {
            var followUp = await _db.FollowUps.FindAsync(id);
            if (followUp is null) return NotFound();

            var userExists = await _db.Users.AnyAsync(u => u.Id == request.AssignedToUserId && u.IsActive);
            if (!userExists)
            {
                return BadRequest(new { message = "Selected member does not exist or is inactive." });
            }

            followUp.AssignedToUserId = request.AssignedToUserId;
            followUp.AssignedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Logs the outcome of a call. Only the assignee or an Admin may do this.</summary>
        [HttpPut("{id}/log")]
        public async Task<IActionResult> Log(int id, [FromBody] LogFollowUpRequest request)
        {
            var followUp = await _db.FollowUps.FindAsync(id);
            if (followUp is null) return NotFound();

            if (!IsAdmin && followUp.AssignedToUserId != CurrentUserId)
            {
                return Forbid();
            }

            if (request.Status == FollowUpStatus.Pending)
            {
                return BadRequest(new { message = "Status must be Completed or Missed when logging a call." });
            }

            followUp.CallDate = request.CallDate;
            followUp.Outcome = request.Outcome;
            followUp.Notes = request.Notes;
            followUp.Status = request.Status;
            followUp.CompletedAt = request.Status == FollowUpStatus.Completed ? DateTime.UtcNow : null;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
