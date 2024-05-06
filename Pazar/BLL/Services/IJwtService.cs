using System.Security.Claims;

namespace BLL.Services;

public interface IJwtService
{
    string GenerateJwtToken(User user);
    ClaimsPrincipal ValidateToken(string token);
}