namespace ChurchHospitalityApp.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toAddress, string toName, string subject, string htmlBody);

        Task SendMemberInviteAsync(string toAddress, string toName, string completeRegistrationUrl);
    }
}
