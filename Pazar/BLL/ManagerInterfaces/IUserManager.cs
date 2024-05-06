using BLL.DTOs;

namespace BLL.ManagerInterfaces;

public interface IUserManager
{
    Task<string> RegisterUserAsync(UserRegisterDTO userDto);
    Task<string?> AuthenticateUserAsync(string email, string password);
    Task UpdateUserDetailsAsync(string uuid, UserUpdateDTO userDto);
    Task DeleteUserAsync(string uuid);
    Task<User> GetUserByIdAsync(string uuid);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task ActivateOrDeactivateUserAsync(string uuid, bool isActive);
    Task<User?> GetUserByEmailAsync(string email);
    Task ChangePasswordAsync(string uuid, string newPassword, string oldPassword);
}