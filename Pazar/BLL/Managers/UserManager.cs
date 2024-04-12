using System.Globalization;
using BLL.RepositoryInterfaces;

namespace BLL;

public class UserManager
{
    private readonly IUserDAO _userDao;

    public UserManager(IUserDAO userDao)
    {
        _userDao = userDao;
    }
    
    public async Task CreateUserAsync(User user)
    {
        await _userDao.CreateUserAsync(user);
    }

    public async Task DeleteUserAsync(string uuid)
    {
        await _userDao.DeleteUserAsync(uuid);
    }

    public async Task UpdateUserAsync(User user)
    {
        await _userDao.UpdateUserAsync(user);
    }

    public async Task<User?> GetUserByIdAsync(string uuid)
    {
        return await _userDao.GetUserByIdAsync(uuid);
    }
    
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userDao.GetAllUsersAsync();
    }
}