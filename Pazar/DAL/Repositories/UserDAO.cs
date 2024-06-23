﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BLL;
using BLL.RepositoryInterfaces;
using DAL.DbContexts;

namespace DAL.Repositories
{
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
            if (Guid.TryParse(uuid, out Guid userGuid))
            {
                var user = await _context.Users.FindAsync(userGuid);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<User?> GetUserByIdAsync(string uuid)
        {
            if (Guid.TryParse(uuid, out Guid userGuid))
            {
                return await _context.Users
                    .Include(u => u.Address)
                    .FirstOrDefaultAsync(u => u.UUID == userGuid);
            }
            return null;
        }



        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}