using System.ComponentModel.DataAnnotations;

namespace ChurchHospitalityApp.Models
{
    /// <summary>A first-time guest captured via the public QR-code form.</summary>
    public class Guest
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Email { get; set; }

        [Required, MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        public DateTime VisitDate { get; set; }

        [MaxLength(300)]
        public string? HowHeardAboutUs { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}
