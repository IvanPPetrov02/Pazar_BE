using System.Security.Claims;
using BLL.DTOs;
using BLL.Encryption;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;
using BLL.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BLL
{
    public class UserManager : IUserManager
    {
        private readonly IUserDAO _userDao;
        private readonly IAddressDAO _addressDao;
        private readonly IJwtService _jwtService;

        public UserManager(IUserDAO userDao, IAddressDAO addressDao, IJwtService jwtService)
        {
            _userDao = userDao;
            _addressDao = addressDao;
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
    try
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

        if (userDto.Address != null)
        {
            var address = await _addressDao.GetAddressByIdAsync(user.Address?.ID ?? 0);
            if (address == null)
            {
                await _addressDao.CreateAddressAsync(userDto.Address);
                user.Address = userDto.Address;
            }
            else
            {
                user.Address = userDto.Address;
                await _addressDao.UpdateAddressAsync(user.Address);
            }
            isUpdated = true;
        }

        if (isUpdated)
        {
            await _userDao.UpdateUserAsync(user);
        }
    }
    catch (DbUpdateException dbEx)
    {
        var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
        Console.WriteLine($"An error occurred while saving the entity changes: {innerException}");
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        throw;
    }
}



        public async Task DeleteUserAsync(string uuid)
        {
            try
            {
                Console.WriteLine($"Attempting to delete user with UUID: {uuid}");

                // Fetch the user by UUID
                var user = await _userDao.GetUserByIdAsync(uuid);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found.");
                }
        
                Console.WriteLine($"User found: {user.Email}");

                // Check if the user has an address and if it exists
                if (user.Address != null)
                {
                    Console.WriteLine($"User has an address with ID: {user.Address.ID}");

                    var address = await _addressDao.GetAddressByIdAsync(user.Address.ID);
                    if (address != null)
                    {
                        await _addressDao.DeleteAddressAsync(address.ID);
                        Console.WriteLine($"Address deleted: {user.Address.ID}");
                    }
                    else
                    {
                        Console.WriteLine($"Address not found: {user.Address.ID}");
                    }
                }
                else
                {
                    Console.WriteLine("User does not have an address.");
                }

                // Delete the user
                await _userDao.DeleteUserAsync(uuid);
                Console.WriteLine($"User deleted: {uuid}");
            }
            catch (InvalidOperationException ex)
            {
                // Log the specific exception
                Console.WriteLine($"Operation error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Log the general exception
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw;
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
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (!PassHash.ValidatePassword(oldPassword, user.Password))
            {
                throw new UnauthorizedAccessException("Old password is incorrect.");
            }

            user.Password = PassHash.HashPassword(newPassword);
            await _userDao.UpdateUserAsync(user);
        }


        public async Task<User?> GetLoggedUserAsync(string userId)
        {
            return await GetUserByIdAsync(userId);
        }





    }
}
