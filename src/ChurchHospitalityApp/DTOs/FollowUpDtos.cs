using System.ComponentModel.DataAnnotations;
using ChurchHospitalityApp.Models;

namespace ChurchHospitalityApp.DTOs
{
    public class FollowUpDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public string GuestFullName { get; set; } = string.Empty;
        public string GuestPhoneNumber { get; set; } = string.Empty;
        public int WeekNumber { get; set; }
        public DateTime ScheduledFor { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CallDate { get; set; }
        public string? Outcome { get; set; }
        public string? Notes { get; set; }
        public DateTime? CompletedAt { get; set; }

        public static FollowUpDto From(FollowUp f) => new()
        {
            Id = f.Id,
            GuestId = f.GuestId,
            GuestFullName = f.Guest?.FullName ?? string.Empty,
            GuestPhoneNumber = f.Guest?.PhoneNumber ?? string.Empty,
            WeekNumber = (int)f.WeekNumber,
            ScheduledFor = f.ScheduledFor,
            AssignedToUserId = f.AssignedToUserId,
            AssignedToUserName = f.AssignedToUser?.FullName,
            Status = f.Status.ToString(),
            CallDate = f.CallDate,
            Outcome = f.Outcome?.ToString(),
            Notes = f.Notes,
            CompletedAt = f.CompletedAt
        };
    }

    public class AssignFollowUpRequest
    {
        [Required]
        public string AssignedToUserId { get; set; } = string.Empty;
    }

    public class LogFollowUpRequest
    {
        [Required]
        public DateTime CallDate { get; set; }

        [Required]
        public CallOutcome Outcome { get; set; }

        [MaxLength(1500)]
        public string? Notes { get; set; }

        /// <summary>Completed (default) or Missed if the guest could not be reached after attempts.</summary>
        public FollowUpStatus Status { get; set; } = FollowUpStatus.Completed;
    }
}
