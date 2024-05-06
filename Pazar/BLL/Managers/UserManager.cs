using BLL.RepositoryInterfaces;
using BLL.DTOs;
using BLL.Encryption;
using BLL.ManagerInterfaces;
using BLL.Services;

namespace BLL;

public class UserManager : IUserManager
{
    private readonly IUserDAO _userDao;
    private readonly IJwtService _jwtService;

    public UserManager(IUserDAO userDao, IJwtService jwtService)
    {
        _userDao = userDao;
        _jwtService = jwtService;
    }

    public async Task<string> RegisterUserAsync(UserRegisterDTO userDto)
    {
        var existingUser = await _userDao.GetUserByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            return "User already exists";
        }

        var newUser = new User
        {
            Email = userDto.Email,
            Password = PassHash.HashPassword(userDto.Password),
            Name = userDto.Name,
            Surname = userDto.Surname
        };

        await _userDao.CreateUserAsync(newUser);
        return "User created";
    }

    public async Task<string?> AuthenticateUserAsync(string email, string password)
    {
        var user = await _userDao.GetUserByEmailAsync(email);
        if (user == null || !PassHash.ValidatePassword(password, user.Password))
        {
            return null;
        }

        return _jwtService.GenerateJwtToken(user);
    }

    public async Task UpdateUserDetailsAsync(string uuid, UserUpdateDTO userDto)
    {
        var user = await _userDao.GetUserByIdAsync(uuid);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        bool isUpdated = false;
        if (userDto.Email != null && userDto.Email != user.Email)
        {
            user.Email = userDto.Email;
            isUpdated = true;
        }
        if (userDto.Name != null && userDto.Name != user.Name)
        {
            user.Name = userDto.Name;
            isUpdated = true;
        }
        if (userDto.Surname != null && userDto.Surname != user.Surname)
        {
            user.Surname = userDto.Surname;
            isUpdated = true;
        }
        if (userDto.Image != null && (user.Image == null || !userDto.Image.SequenceEqual(user.Image)))
        {
            user.Image = userDto.Image;
            isUpdated = true;
        }
        if (userDto.IsActive != user.IsActive)
        {
            user.IsActive = userDto.IsActive;
            isUpdated = true;
        }
        if (isUpdated)
            await _userDao.UpdateUserAsync(user);
    }

    public async Task DeleteUserAsync(string uuid)
    {
        await _userDao.DeleteUserAsync(uuid);
    }

    public async Task<User> GetUserByIdAsync(string uuid)
    {
        return await _userDao.GetUserByIdAsync(uuid);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userDao.GetAllUsersAsync();
    }

    public async Task ActivateOrDeactivateUserAsync(string uuid, bool isActive)
    {
        var user = await _userDao.GetUserByIdAsync(uuid);
        if (user != null)
        {
            user.IsActive = isActive;
            await _userDao.UpdateUserAsync(user);
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userDao.GetUserByEmailAsync(email);
    }

    public async Task ChangePasswordAsync(string uuid, string newPassword, string oldPassword)
    {
        var user = await _userDao.GetUserByIdAsync(uuid);
        if (user != null && PassHash.ValidatePassword(oldPassword, user.Password))
        {
            user.Password = PassHash.HashPassword(newPassword);
            await _userDao.UpdateUserAsync(user);
        }
    }
}
