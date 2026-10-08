using ChurchHospitalityApp.Models;

namespace ChurchHospitalityApp.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
