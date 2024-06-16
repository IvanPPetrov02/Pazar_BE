using System.Security.Claims;

namespace BLL.Services;

public interface IJwtService
{
    string GenerateJwtToken(User user);
    // bool ValidateToken(string token, out ClaimsPrincipal? principal);
    // bool IsAdmin(ClaimsPrincipal principal);
    //bool IsAdminOrOwner(ClaimsPrincipal user, string ownerId);
}
