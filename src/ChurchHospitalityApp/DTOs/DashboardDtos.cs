namespace ChurchHospitalityApp.DTOs
{
    public class DashboardSummaryDto
    {
        public int PendingFollowUps { get; set; }
        public int CompletedFollowUps { get; set; }
        public int MissedFollowUps { get; set; }

        // Populated for Admins only
        public int? TotalGuestsThisMonth { get; set; }
        public int? TotalActiveMembers { get; set; }
        public int? UnassignedFollowUps { get; set; }

        public string Scope { get; set; } = "mine"; // "mine" | "all"
    }
}
