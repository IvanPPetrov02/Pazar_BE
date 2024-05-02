using BLL;
using BLL.RepositoryInterfaces;
using DAL.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserDAO : IUserDAO
{
    private readonly AppDbContext _context;

    public UserDAO(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(string uuid)
    {
        var user = await _context.Users.FindAsync(uuid);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User> GetUserByIdAsync(string uuid)
    {
        return await _context.Users.FindAsync(uuid);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}