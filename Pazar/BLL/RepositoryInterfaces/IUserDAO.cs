﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.RepositoryInterfaces;

public interface IUserDAO
{
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(string uuid);
    Task<User> GetUserByIdAsync(string uuid);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByEmailAsync(string email);
}