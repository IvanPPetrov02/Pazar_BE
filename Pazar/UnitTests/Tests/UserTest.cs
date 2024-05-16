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
        private Mock<IJwtService> _mockJwtService;

        [SetUp]
        public void Setup()
        {
            _userDaoFake = new UserDAOFake();
            _mockJwtService = new Mock<IJwtService>();
            _userManager = new UserManager(_userDaoFake, _mockJwtService.Object);
        }

        [Test]
        public async Task RegisterUserAsync_ShouldReturnUserCreated_WhenUserDoesNotExist()
        {
            // Arrange
            var newUserDto = new UserRegisterDTO
            {
                Email = "newuser@example.com",
                Password = "newpassword",
                Name = "New",
                Surname = "User"
            };

            // Act
            var result = await _userManager.RegisterUserAsync(newUserDto);

            // Assert
            Assert.AreEqual("User created", result);
        }

        [Test]
        public async Task RegisterUserAsync_ShouldReturnUserAlreadyExists_WhenUserExists()
        {
            // Arrange
            var existingUserDto = new UserRegisterDTO
            {
                Email = "test1@example.com",
                Password = "password1",
                Name = "Test1",
                Surname = "User1"
            };

            // Act
            var result = await _userManager.RegisterUserAsync(existingUserDto);

            // Assert
            Assert.AreEqual("User already exists", result);
        }

        [Test]
        public async Task AuthenticateUserAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var email = "test1@example.com";
            var password = "password1";
            var token = "valid-token";
            _mockJwtService.Setup(x => x.GenerateJwtToken(It.IsAny<User>())).Returns(token);

            // Act
            var result = await _userManager.AuthenticateUserAsync(email, password);

            // Assert
            Assert.AreEqual(token, result);
        }

        [Test]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
        {
            // Arrange
            var email = "test1@example.com";
            var password = "wrongpassword";

            // Act
            var result = await _userManager.AuthenticateUserAsync(email, password);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateUserDetailsAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var userDto = new UserUpdateDTO
            {
                Name = "UpdatedName",
                Surname = "UpdatedSurname"
            };

            // Act
            await _userManager.UpdateUserDetailsAsync(user.UUID.ToString(), userDto);
            var updatedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());

            // Assert
            Assert.AreEqual("UpdatedName", updatedUser.Name);
            Assert.AreEqual("UpdatedSurname", updatedUser.Surname);
        }

        [Test]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
        {
            // Arrange
            var user = (await _userDaoFake.GetAllUsersAsync()).First();

            // Act
            await _userManager.DeleteUserAsync(user.UUID.ToString());
            var deletedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());

            // Assert
            Assert.IsNull(deletedUser);
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = (await _userDaoFake.GetAllUsersAsync()).First();

            // Act
            var result = await _userManager.GetUserByIdAsync(user.UUID.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(user.UUID, result.UUID);
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Act
            var result = await _userManager.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public async Task ActivateOrDeactivateUserAsync_ShouldChangeUserStatus_WhenUserExists()
        {
            // Arrange
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var initialStatus = user.IsActive;

            // Act
            await _userManager.ActivateOrDeactivateUserAsync(user.UUID.ToString(), !initialStatus);
            var updatedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());

            // Assert
            Assert.AreNotEqual(initialStatus, updatedUser.IsActive);
        }

        [Test]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var email = "test1@example.com";

            // Act
            var result = await _userManager.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(email, result.Email);
        }

        [Test]
        public async Task ChangePasswordAsync_ShouldChangePassword_WhenOldPasswordIsCorrect()
        {
            // Arrange
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var oldPassword = "password1";
            var newPassword = "newpassword1";

            // Act
            await _userManager.ChangePasswordAsync(user.UUID.ToString(), newPassword, oldPassword);
            var updatedUser = await _userDaoFake.GetUserByIdAsync(user.UUID.ToString());

            // Assert
            Assert.True(PassHash.ValidatePassword(newPassword, updatedUser.Password));
        }

        [Test]
        public async Task GetLoggedUserAsync_ShouldReturnUser_WhenTokenIsValid()
        {
            // Arrange
            var user = (await _userDaoFake.GetAllUsersAsync()).First();
            var token = "valid-token";
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UUID.ToString())
            }));
            _mockJwtService.Setup(x => x.ValidateToken(token)).Returns(claims);

            // Act
            var result = await _userManager.GetLoggedUserAsync(token);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(user.UUID, result.UUID);
        }
    }
}
