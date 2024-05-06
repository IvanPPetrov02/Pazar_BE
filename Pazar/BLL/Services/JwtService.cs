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
            // Retrieve the secret key from the configuration
            _secretKey = configuration["AppSettings:Token"] ?? throw new InvalidOperationException("JWT secret key must be set.");
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            // Create claims based on user data
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UUID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1), // Adjust expiration as necessary
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Create and write the JWT token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // Skip issuer validation if not required
                ValidateAudience = false, // Skip audience validation if not required
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                // Validate the incoming JWT token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal; // Return ClaimsPrincipal if the token is valid
            }
            catch (SecurityTokenException)
            {
                // Token validation failed
                return null;
            }
        }
    }
}
