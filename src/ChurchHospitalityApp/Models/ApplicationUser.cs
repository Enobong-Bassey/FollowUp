using Microsoft.AspNetCore.Identity;

namespace ChurchHospitalityApp.Models
{
    /// <summary>
    /// Extends ASP.NET Core Identity's user with the fields the Hospitality Unit needs.
    /// Email/PhoneNumber/PasswordHash/EmailConfirmed are inherited from IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string? ProfilePhotoUrl { get; set; }

        /// <summary>Admins can deactivate a member without deleting their history.</summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FollowUp> AssignedFollowUps { get; set; } = new List<FollowUp>();
    }
}
