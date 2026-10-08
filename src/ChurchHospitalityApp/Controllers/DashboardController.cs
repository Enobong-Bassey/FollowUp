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
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary([FromQuery] string scope = "mine")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole(AppRoles.Admin);
            var orgWide = scope == "all" && isAdmin;

            var followUps = _db.FollowUps.AsQueryable();
            if (!orgWide)
            {
                followUps = followUps.Where(f => f.AssignedToUserId == userId);
            }

            var dto = new DashboardSummaryDto
            {
                Scope = orgWide ? "all" : "mine",
                PendingFollowUps = await followUps.CountAsync(f => f.Status == FollowUpStatus.Pending),
                CompletedFollowUps = await followUps.CountAsync(f => f.Status == FollowUpStatus.Completed),
                MissedFollowUps = await followUps.CountAsync(f => f.Status == FollowUpStatus.Missed)
            };

            if (isAdmin)
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                dto.TotalGuestsThisMonth = await _db.Guests.CountAsync(g => g.CreatedAt >= startOfMonth);
                dto.TotalActiveMembers = await _db.Users.CountAsync(u => u.IsActive);
                dto.UnassignedFollowUps = await _db.FollowUps.CountAsync(f => f.AssignedToUserId == null);
            }

            return Ok(dto);
        }
    }
}
