using System.Security.Claims;
using BLL;
using BLL.Services;

namespace UnitTests.FakeServices
{
    public class JwtServiceFake : IJwtService
    {
        public string GenerateJwtToken(User user)
        {
            return "fake-jwt-token";
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "fake-user-id"),
                new Claim(ClaimTypes.Email, "fake@example.com"),
                new Claim(ClaimTypes.Role, "User")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }
    }
}