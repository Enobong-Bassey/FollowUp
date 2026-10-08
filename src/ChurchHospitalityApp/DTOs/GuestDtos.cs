using System.ComponentModel.DataAnnotations;

namespace ChurchHospitalityApp.DTOs
{
    /// <summary>Submitted by the guest from the public QR-code form (no auth).</summary>
    public class CreateGuestRequest
    {
        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [Required, MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        public DateTime? VisitDate { get; set; }

        [MaxLength(300)]
        public string? HowHeardAboutUs { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class GuestSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PendingFollowUps { get; set; }
        public int CompletedFollowUps { get; set; }
    }

    public class GuestDetailDto : GuestSummaryDto
    {
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? HowHeardAboutUs { get; set; }
        public string? Notes { get; set; }
        public List<FollowUpDto> FollowUps { get; set; } = new();
    }
}
