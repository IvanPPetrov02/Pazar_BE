﻿using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL;
using BLL.DTOs;
using System.Threading.Tasks;
using BLL.ManagerInterfaces;
using BLL.Services;
using CustomAuthorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Pazar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IJwtService _jwtService;

        public UserController(IUserManager userManager, ILogger<UserController> logger, IJwtService jwtService)
        {
            _userManager = userManager;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userDto)
        {
            _logger.LogInformation("Register endpoint hit");
            _logger.LogInformation($"Email: {userDto.Email}, Name: {userDto.Name}, Surname: {userDto.Surname}");

            try
            {
                var result = await _userManager.RegisterUserAsync(userDto);
                if (result == "User created")
                {
                    _logger.LogInformation("User created successfully");
                    return Ok(new { message = result });
                }
                else
                {
                    _logger.LogWarning($"Registration failed: {result}");
                    return BadRequest(new { message = result });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the registration");
                return StatusCode(500, "Internal server error");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
        {
            var token = await _userManager.AuthenticateUserAsync(loginDto.Email, loginDto.Password);
            if (token != null)
            {
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
                return Ok(new { token });
            }
            return Unauthorized("Authentication failed");
        }

        [Authorize]
        [HttpGet("{uuid}")]
        public async Task<IActionResult> GetUser(string uuid)
        {
            var user = await _userManager.GetUserByIdAsync(uuid);
            return user != null ? Ok(user) : NotFound();
        }

        [Authorize]
        [HttpPut("{uuid}")]
        public async Task<IActionResult> UpdateUser(string uuid, [FromBody] UserUpdateDTO userDto)
        {
            try
            {
                await _userManager.UpdateUserDetailsAsync(uuid, userDto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user");
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{uuid}")]
        [ServiceFilter(typeof(IsTheUserOrAdminAttribute))]
        public async Task<IActionResult> DeleteUser(string uuid)
        {
            try
            {
                // Attempt to delete the user
                await _userManager.DeleteUserAsync(uuid);
                // Return a NoContent status if the deletion is successful
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                // Log a warning if the user was not found
                _logger.LogWarning("User not found for deletion: " + ex.Message);
                // Return a NotFound status with the exception message
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log an error if an unexpected exception occurs
                _logger.LogError(ex, "An error occurred while deleting the user");
                // Return a 500 Internal Server Error status with the exception message
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }


        [Authorize]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetLoggedUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var user = await _userManager.GetLoggedUserAsync(userIdClaim.Value);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid or expired token." });
            }

            return Ok(user);
        }

        [Authorize]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("activate/{uuid}")]
        public async Task<IActionResult> ActivateUser(string uuid)
        {
            await _userManager.ActivateOrDeactivateUserAsync(uuid, true);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("deactivate/{uuid}")]
        public async Task<IActionResult> DeactivateUser(string uuid)
        {
            await _userManager.ActivateOrDeactivateUserAsync(uuid, false);
            return Ok();
        }

        [Authorize]
        [HttpPost("change-password/{uuid}")]
        [ServiceFilter(typeof(IsTheUserOrAdminAttribute))]
        public async Task<IActionResult> ChangePassword(string uuid, [FromBody] UserPasswordChangeDTO passwordChangeDto)
        {
            try
            {
                await _userManager.ChangePasswordAsync(uuid, passwordChangeDto.NewPassword, passwordChangeDto.OldPassword);
                return Ok(new { Message = "Password successfully changed." });
            }
            catch (InvalidOperationException ex)
            {
                // Return 404
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Return 401
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Return 400
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            };

            Response.Cookies.Append("jwt", "", cookieOptions);

            return Ok("success");
        }
        
    }
}
