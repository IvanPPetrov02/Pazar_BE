using BLL.RepositoryInterfaces;
using BLL.DTOs;
using BLL.Encryption;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BLL.ManagerInterfaces;
using Microsoft.IdentityModel.Tokens;

namespace BLL;

public class UserManager: IUserManager
{
    private readonly IUserDAO _userDao;

    public UserManager(IUserDAO userDao)
    {
        _userDao = userDao;
    }

    public async Task<string> RegisterUserAsync(UserRegisterDTO userDto)
    {
        var existingUser = await _userDao.GetUserByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            return "User already exists";
        }

        var newUser = new User(userDto.Email, PassHash.HashPassword(userDto.Password));
        await _userDao.CreateUserAsync(newUser);
        return "User created";
    }

    public async Task<string?> AuthenticateUserAsync(string email, string password)
    {
        var user = await _userDao.GetUserByEmailAsync(email);
        if (user == null) return null;

        bool isPasswordValid = PassHash.ValidatePassword(password, user.Password);
        if (!isPasswordValid) return null;

        return GenerateJwtToken(user);
    }
    
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("JWT secret key must be set and not empty.");
        }
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UUID.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task UpdatePasswordAsync(string uuid, string newPassword)
    {
        var user = await _userDao.GetUserByIdAsync(uuid);
        if (user != null)
        {
            user.Password = PassHash.HashPassword(newPassword);
            await _userDao.UpdateUserAsync(user);
        }
    }

    public async Task UpdateEmailAsync(string uuid, string newEmail)
    {
        var user = await _userDao.GetUserByIdAsync(uuid);
        if (user != null)
        {
            user.Email = newEmail;
            await _userDao.UpdateUserAsync(user);
        }
    }

    public async Task UpdatePersonalInfoAsync(string uuid, string newName, string newSurname, string newPhone, byte[] newImage)
    {
        var user = await _userDao.GetUserByIdAsync(uuid);
        if (user != null)
        {
            user.Name = newName;
            user.Surname = newSurname;
            user.Phone = newPhone;
            user.Image = newImage;
            await _userDao.UpdateUserAsync(user);
        }
    }

    public async Task UpdateIsActiveAsync(string uuid, bool newIsActive)
    {
        var user = await _userDao.GetUserByIdAsync(uuid);
        if (user != null)
        {
            user.IsActive = newIsActive;
            await _userDao.UpdateUserAsync(user);
        }
    }

    public async Task<User?> GetUserByIdAsync(string uuid)
    {
        return await _userDao.GetUserByIdAsync(uuid);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userDao.GetAllUsersAsync();
    }

    public async Task DeleteUserAsync(string uuid)
    {
        await _userDao.DeleteUserAsync(uuid);
    }
}
