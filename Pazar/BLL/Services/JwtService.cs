using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secretKey;

        public JwtService(IConfiguration configuration)
        {
            _secretKey = configuration["AppSettings:Token"] ??
                         throw new InvalidOperationException("JWT secret key must be set.");
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UUID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token, out ClaimsPrincipal? principal)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (SecurityTokenException)
            {
                principal = null;
                return false;
            }
        }

        public bool IsAdmin(ClaimsPrincipal principal)
        {
            return principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value == "Admin";
        }

        public bool IsAdminOrOwner(ClaimsPrincipal principal, string userId)
        {
            return IsAdmin(principal) ||
                   principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value == userId;
        }
    }
}
