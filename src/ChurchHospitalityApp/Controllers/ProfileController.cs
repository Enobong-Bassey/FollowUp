using System.Security.Claims;
using ChurchHospitalityApp.DTOs;
using ChurchHospitalityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChurchHospitalityApp.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private static readonly string[] AllowedPhotoExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxPhotoSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<ActionResult<MemberDto>> Get()
        {
            var user = await _userManager.FindByIdAsync(CurrentUserId);
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new MemberDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = roles.Contains(AppRoles.Admin) ? AppRoles.Admin : AppRoles.User,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(CurrentUserId);
            if (user is null) return NotFound();

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            if (file.Length > MaxPhotoSizeBytes)
            {
                return BadRequest(new { message = "Photo must be smaller than 5MB." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedPhotoExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Only JPG, PNG, or WEBP images are allowed." });
            }

            var user = await _userManager.FindByIdAsync(CurrentUserId);
            if (user is null) return NotFound();

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profile-photos");
            Directory.CreateDirectory(uploadsFolder);

            // Remove an older photo with a different extension so files don't pile up
            foreach (var old in Directory.GetFiles(uploadsFolder, $"{user.Id}.*"))
            {
                System.IO.File.Delete(old);
            }

            var fileName = $"{user.Id}{extension}";
            await using (var stream = new FileStream(Path.Combine(uploadsFolder, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Version query string defeats browser caching after a re-upload
            user.ProfilePhotoUrl = $"/uploads/profile-photos/{fileName}?v={DateTime.UtcNow.Ticks}";
            await _userManager.UpdateAsync(user);

            return Ok(new { profilePhotoUrl = user.ProfilePhotoUrl });
        }
    }
}
