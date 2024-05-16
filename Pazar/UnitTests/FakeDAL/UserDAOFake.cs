using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using BLL.RepositoryInterfaces;
using BLL.Encryption;

namespace UnitTests.FakeDAL
{
    public class UserDAOFake : IUserDAO
    {
        private List<User> _users;

        public UserDAOFake()
        {
            _users = new List<User>
            {
                new User
                {
                    UUID = Guid.NewGuid(),
                    Email = "test1@example.com",
                    Password = PassHash.HashPassword("password1"),
                    Name = "Test1",
                    Surname = "User1",
                    IsActive = true
                },
                new User
                {
                    UUID = Guid.NewGuid(),
                    Email = "test2@example.com",
                    Password = PassHash.HashPassword("password2"),
                    Name = "Test2",
                    Surname = "User2",
                    IsActive = true
                },
                new User
                {
                    UUID = Guid.NewGuid(),
                    Email = "test3@example.com",
                    Password = PassHash.HashPassword("password3"),
                    Name = "Test3",
                    Surname = "User3",
                    IsActive = false
                }
            };
        }

        public Task CreateUserAsync(User user)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.UUID == user.UUID);
            if (existingUser != null)
            {
                _users.Remove(existingUser);
                _users.Add(user);
            }
            return Task.CompletedTask;
        }

        public Task DeleteUserAsync(string uuid)
        {
            var user = _users.FirstOrDefault(u => u.UUID.ToString() == uuid);
            if (user != null)
            {
                _users.Remove(user);
            }
            return Task.CompletedTask;
        }

        public Task<User> GetUserByIdAsync(string uuid)
        {
            var user = _users.FirstOrDefault(u => u.UUID.ToString() == uuid);
            return Task.FromResult(user);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return Task.FromResult(_users.AsEnumerable());
        }

        public Task<User> GetUserByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            return Task.FromResult(user);
        }
    }
}
