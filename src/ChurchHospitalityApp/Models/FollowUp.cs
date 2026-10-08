using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchHospitalityApp.Models
{
    /// <summary>
    /// One follow-up task for a guest (exactly two per guest: week 1 and week 2).
    /// Created automatically with the guest, assigned by an Admin, and logged by the assignee.
    /// </summary>
    public class FollowUp
    {
        public int Id { get; set; }

        public int GuestId { get; set; }
        public Guest? Guest { get; set; }

        public FollowUpWeek WeekNumber { get; set; }

        /// <summary>Visit date + 7 days (week 1) or + 14 days (week 2).</summary>
        public DateTime ScheduledFor { get; set; }

        public string? AssignedToUserId { get; set; }
        [ForeignKey(nameof(AssignedToUserId))]
        public ApplicationUser? AssignedToUser { get; set; }

        public DateTime? AssignedAt { get; set; }

        public FollowUpStatus Status { get; set; } = FollowUpStatus.Pending;

        // --- Logged call details ---
        public DateTime? CallDate { get; set; }
        public CallOutcome? Outcome { get; set; }

        [MaxLength(1500)]
        public string? Notes { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
