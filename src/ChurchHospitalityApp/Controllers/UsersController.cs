using System.Web;
using ChurchHospitalityApp.DTOs;
using ChurchHospitalityApp.Models;
using ChurchHospitalityApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchHospitalityApp.Controllers
{
    /// <summary>Admin-only endpoints for managing Hospitality Unit members.</summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public UsersController(UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration config)
        {
            _userManager = userManager;
            _emailService = emailService;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<MemberDto>>> GetAll()
        {
            var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
            var result = new List<MemberDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(ToDto(user, roles.Contains(AppRoles.Admin) ? AppRoles.Admin : AppRoles.User));
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
        {
            var role = request.Role == AppRoles.Admin ? AppRoles.Admin : AppRoles.User;

            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                return Conflict(new { message = "A member with this email already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Created without a password; the member sets it via the invite link.
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { errors = createResult.Errors.Select(e => e.Description) });
            }

            await _userManager.AddToRoleAsync(user, role);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var baseUrl = _config["App:FrontendBaseUrl"]?.TrimEnd('/');
            var link = $"{baseUrl}/complete-registration.html?userId={HttpUtility.UrlEncode(user.Id)}&token={HttpUtility.UrlEncode(token)}";

            try
            {
                await _emailService.SendMemberInviteAsync(user.Email!, user.FullName, link);
            }
            catch
            {
                return StatusCode(201, new
                {
                    member = ToDto(user, role),
                    warning = "Member created, but the invitation email could not be sent. Please check the SMTP settings."
                });
            }

            return StatusCode(201, ToDto(user, role));
        }

        [HttpPut("{id}/active")]
        public async Task<IActionResult> SetActive(string id, [FromBody] SetMemberActiveRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            user.IsActive = request.IsActive;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        private static MemberDto ToDto(ApplicationUser user, string role) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Role = role,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            CreatedAt = user.CreatedAt
        };
    }
}
