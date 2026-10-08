namespace ChurchHospitalityApp.Models
{
    /// <summary>The two application roles (also stored as ASP.NET Identity roles).</summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }

    public enum FollowUpStatus
    {
        Pending = 0,
        Completed = 1,
        Missed = 2
    }

    public enum FollowUpWeek
    {
        WeekOne = 1,
        WeekTwo = 2
    }

    public enum CallOutcome
    {
        Reached = 0,
        NoAnswer = 1,
        LeftVoicemail = 2,
        WrongNumber = 3,
        RequestedNoContact = 4,
        Other = 5
    }
}
