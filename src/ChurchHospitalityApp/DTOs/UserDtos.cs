using System.ComponentModel.DataAnnotations;

namespace ChurchHospitalityApp.DTOs
{
    /// <summary>Submitted by an Admin to add a Hospitality Unit member.</summary>
    public class CreateMemberRequest
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>"Admin" or "User". Defaults to "User".</summary>
        public string Role { get; set; } = "User";
    }

    public class MemberDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class SetMemberActiveRequest
    {
        public bool IsActive { get; set; }
    }
}
