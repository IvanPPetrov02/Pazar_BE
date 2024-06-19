using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL;
using BLL.DTOs;
using BLL.Encryption;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;
using BLL.Services;
using Moq;
using NUnit.Framework;
using UnitTests.FakeDAL;

namespace UnitTests
{
    [TestFixture]
    public class UserManagerTests
    {
        private UserManager _userManager;
        private UserDAOFake _userDaoFake;
        private AddressDAOFake _addressDaoFake;
        private Mock<IJwtService> _mockJwtService;

        [SetUp]
        public void Setup()
        {
            _userDaoFake = new UserDAOFake();
            _addressDaoFake = new AddressDAOFake();
            _mockJwtService = new Mock<IJwtService>();
            _userManager = new UserManager(_userDaoFake, _addressDaoFake, _mockJwtService.Object);
        }

        [Test]
        public async Task RegisterUserAsync_ShouldReturnUserCreated_WhenUserDoesNotExist()
        {
            var newUserDto = new UserRegisterDTO
            {
                Email = "newuser@example.com",
                Password = "newpassword",
                Name = "New",
                Surname = "User"
            };

            var result = await _userManager.RegisterUserAsync(newUserDto);

            Assert.AreEqual("User created", result);
        }

        [Test]
        public async Task RegisterUserAsync_ShouldReturnUserAlreadyExists_WhenUserExists()
        {
            var existingUserDto = new UserRegisterDTO
            {
                Email = "test1@example.com",
                Password = "password1",
                Name = "Test1",
                Surname = "User1"
            };

            var result = await _userManager.RegisterUserAsync(existingUserDto);

            Assert.AreEqual("User already exists", result);
        }

        [Test]
        public async Task AuthenticateUserAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var email = "test1@example.com";
            var password = "password1";
            var token = "valid-token";
            _mockJwtService.Setup(x => x.GenerateJwtToken(It.IsAny<User>())).Returns(token);

            var result = await _userManager.AuthenticateUserAsync(email, password);

            Assert.AreEqual(token, result);
        }

        [Test]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
        {
            var email = "test1@example.com";
            var password = "wrongpassword";

            var result = await _userManager.AuthenticateUserAsync(email, password);

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateUserDetailsAsync_ShouldUpdateUser_WhenUserExists()
        {
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var userDto = new UserUpdateDTO
            {
                Name = "UpdatedName",
                Surname = "UpdatedSurname"
            };

            await _userManager.UpdateUserDetailsAsync(user.UUID.ToString(), userDto);
            var updatedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());

            Assert.AreEqual("UpdatedName", updatedUser.Name);
            Assert.AreEqual("UpdatedSurname", updatedUser.Surname);
        }

        [Test]
        public async Task UpdateUserDetailsAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            var userDto = new UserUpdateDTO
            {
                Name = "UpdatedName",
                Surname = "UpdatedSurname"
            };

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userManager.UpdateUserDetailsAsync("nonexistent-uuid", userDto));
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteUserAndAddress_WhenUserExists()
        {
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var address = new Address
            {
                ID = 1,
                Country = "Country1",
                City = "City1",
                Street = "Street1",
                Number = "Number1",
                ZipCode = "ZipCode1"
            };
            user.Address = address;

            await _userManager.DeleteUserAsync(user.UUID.ToString());
            var deletedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());
            var deletedAddress = await _addressDaoFake.GetAddressByIdAsync(address.ID);

            Assert.IsNull(deletedUser);
            Assert.IsNull(deletedAddress);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var user = (await _userDaoFake.GetAllUsersAsync()).First();

            var result = await _userManager.GetUserByIdAsync(user.UUID.ToString());

            Assert.NotNull(result);
            Assert.AreEqual(user.UUID, result.UUID);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            var result = await _userManager.GetUserByIdAsync("nonexistent-uuid");

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            var result = await _userManager.GetAllUsersAsync();

            Assert.NotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public async Task ActivateOrDeactivateUserAsync_ShouldChangeUserStatus_WhenUserExists()
        {
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var initialStatus = user.IsActive;

            await _userManager.ActivateOrDeactivateUserAsync(user.UUID.ToString(), !initialStatus);
            var updatedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());

            Assert.AreNotEqual(initialStatus, updatedUser.IsActive);
        }

        [Test]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            var email = "test1@example.com";

            var result = await _userManager.GetUserByEmailAsync(email);

            Assert.NotNull(result);
            Assert.AreEqual(email, result.Email);
        }

        [Test]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            var result = await _userManager.GetUserByEmailAsync("nonexistent@example.com");

            Assert.IsNull(result);
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldChangePassword_WhenOldPasswordIsCorrect()
        {
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var oldPassword = "password1";
            var newPassword = "newpassword1";

            await _userManager.ChangePasswordAsync(user.UUID.ToString(), newPassword, oldPassword);
            var updatedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());

            Assert.True(PassHash.ValidatePassword(newPassword, updatedUser.Password));
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userManager.ChangePasswordAsync("nonexistent-uuid", "newpassword", "oldpassword"));
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldThrowException_WhenOldPasswordIsIncorrect()
        {
            var user = (await _userDaoFake.GetAllUsersAsync()).First();

            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _userManager.ChangePasswordAsync(user.UUID.ToString(), "newpassword", "wrongpassword"));
        }

        [Test]
        public async Task GetLoggedUserAsync_ShouldReturnUser_WhenTokenIsValid()
        {
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UUID.ToString())
            }));

            var result = await _userManager.GetLoggedUserAsync(user.UUID.ToString());

            Assert.NotNull(result);
            Assert.AreEqual(user.UUID, result.UUID);
        }

        [Test]
        public async Task GetLoggedUserAsync_ShouldReturnNull_WhenTokenIsInvalid()
        {
            var result = await _userManager.GetLoggedUserAsync("nonexistent-uuid");

            Assert.IsNull(result);
        }
    }
}