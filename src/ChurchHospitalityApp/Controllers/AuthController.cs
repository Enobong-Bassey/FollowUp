using ChurchHospitalityApp.DTOs;
using ChurchHospitalityApp.Models;
using ChurchHospitalityApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChurchHospitalityApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Reached from the emailed invite link. Confirms the email, sets the password,
        /// and captures the member's phone number.
        /// </summary>
        [HttpPost("complete-registration")]
        public async Task<IActionResult> CompleteRegistration([FromBody] CompleteRegistrationRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return BadRequest(new { message = "Invalid or expired registration link." });
            }

            if (user.EmailConfirmed && await _userManager.HasPasswordAsync(user))
            {
                return BadRequest(new { message = "This account has already completed registration. Please log in." });
            }

            var confirmResult = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!confirmResult.Succeeded)
            {
                return BadRequest(new { message = "Invalid or expired registration link." });
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password);
            if (!addPasswordResult.Succeeded)
            {
                return BadRequest(new { errors = addPasswordResult.Errors.Select(e => e.Description) });
            }

            user.PhoneNumber = request.PhoneNumber;
            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Registration complete. You can now log in." });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.IsActive)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (!user.EmailConfirmed || !await _userManager.HasPasswordAsync(user))
            {
                return Unauthorized(new { message = "Please complete your registration using the link emailed to you before logging in." });
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiresAt) = _tokenService.GenerateToken(user, roles);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.Contains(AppRoles.Admin) ? AppRoles.Admin : AppRoles.User,
                ProfilePhotoUrl = user.ProfilePhotoUrl
            });
        }
    }
}
