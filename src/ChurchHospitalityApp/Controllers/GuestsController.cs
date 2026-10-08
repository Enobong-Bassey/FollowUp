using ChurchHospitalityApp.Data;
using ChurchHospitalityApp.DTOs;
using ChurchHospitalityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchHospitalityApp.Controllers
{
    [ApiController]
    [Route("api/guests")]
    public class GuestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public GuestsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Public endpoint reached by scanning the QR code (no auth).
        /// Creates the guest and auto-generates their week 1 and week 2 follow-ups.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateGuestRequest request)
        {
            var visitDate = (request.VisitDate ?? DateTime.UtcNow).Date;

            var guest = new Guest
            {
                FullName = request.FullName.Trim(),
                Email = request.Email,
                PhoneNumber = request.PhoneNumber.Trim(),
                Address = request.Address,
                Gender = request.Gender,
                VisitDate = visitDate,
                HowHeardAboutUs = request.HowHeardAboutUs,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                // Added together with the guest so both rows save in one transaction
                FollowUps =
                {
                    new FollowUp
                    {
                        WeekNumber = FollowUpWeek.WeekOne,
                        ScheduledFor = visitDate.AddDays(7),
                        Status = FollowUpStatus.Pending
                    },
                    new FollowUp
                    {
                        WeekNumber = FollowUpWeek.WeekTwo,
                        ScheduledFor = visitDate.AddDays(14),
                        Status = FollowUpStatus.Pending
                    }
                }
            };

            _db.Guests.Add(guest);
            await _db.SaveChangesAsync();

            return StatusCode(201, new { message = "Thank you! We're glad you joined us." });
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<GuestSummaryDto>>> GetAll([FromQuery] string? search)
        {
            var query = _db.Guests.Include(g => g.FollowUps).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(g =>
                    g.FullName.Contains(search) ||
                    (g.Email != null && g.Email.Contains(search)) ||
                    g.PhoneNumber.Contains(search));
            }

            var guests = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();

            return Ok(guests.Select(g => new GuestSummaryDto
            {
                Id = g.Id,
                FullName = g.FullName,
                Email = g.Email,
                PhoneNumber = g.PhoneNumber,
                VisitDate = g.VisitDate,
                CreatedAt = g.CreatedAt,
                PendingFollowUps = g.FollowUps.Count(f => f.Status == FollowUpStatus.Pending),
                CompletedFollowUps = g.FollowUps.Count(f => f.Status == FollowUpStatus.Completed)
            }).ToList());
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<GuestDetailDto>> GetById(int id)
        {
            var guest = await _db.Guests
                .Include(g => g.FollowUps)
                .ThenInclude(f => f.AssignedToUser)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guest is null) return NotFound();

            return Ok(new GuestDetailDto
            {
                Id = guest.Id,
                FullName = guest.FullName,
                Email = guest.Email,
                PhoneNumber = guest.PhoneNumber,
                Address = guest.Address,
                Gender = guest.Gender,
                VisitDate = guest.VisitDate,
                CreatedAt = guest.CreatedAt,
                HowHeardAboutUs = guest.HowHeardAboutUs,
                Notes = guest.Notes,
                PendingFollowUps = guest.FollowUps.Count(f => f.Status == FollowUpStatus.Pending),
                CompletedFollowUps = guest.FollowUps.Count(f => f.Status == FollowUpStatus.Completed),
                FollowUps = guest.FollowUps
                    .OrderBy(f => f.WeekNumber)
                    .Select(f => { f.Guest = guest; return FollowUpDto.From(f); })
                    .ToList()
            });
        }
    }
}
