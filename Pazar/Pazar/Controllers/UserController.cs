﻿using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL;
using BLL.DTOs;
using System.Threading.Tasks;
using BLL.ManagerInterfaces;
using BLL.Services;

namespace Pazar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserManager userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
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
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
        {
            var token = await _userManager.AuthenticateUserAsync(loginDto.Email, loginDto.Password);
            if (token != null)
            {
                Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true, Secure = true });
                return Ok(new { token });
            }
            else
            {
                return Unauthorized("Authentication failed");
            }
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
                return StatusCode(500, ex.Message);
            }
        }
        
        [Authorize]
        [HttpDelete("{uuid}")]
        public async Task<IActionResult> DeleteUser(string uuid)
        {
            await _userManager.DeleteUserAsync(uuid);
            return Ok();
        }

        [Authorize]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetLoggedUser()
        {
            try
            {
                // Try to retrieve the JWT from the Authorization header
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                string jwt = null;

                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
                {
                    jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
                }
                else
                {
                    // If the JWT is not in the Authorization header, check the cookies
                    jwt = Request.Cookies["jwt"];
                }

                if (string.IsNullOrEmpty(jwt))
                {
                    return Unauthorized(new { message = "Missing or invalid authorization token." });
                }

                var user = await _userManager.GetLoggedUserAsync(jwt);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid or expired token." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }



        [Authorize]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpPost("activate/{uuid}")]
        public async Task<IActionResult> ActivateUser(string uuid)
        {
            await _userManager.ActivateOrDeactivateUserAsync(uuid, true);
            return Ok();
        }

        [Authorize]
        [HttpPost("deactivate/{uuid}")]
        public async Task<IActionResult> DeactivateUser(string uuid)
        {
            await _userManager.ActivateOrDeactivateUserAsync(uuid, false);
            return Ok();
        }

        [Authorize]
        [HttpPost("change-password/{uuid}")]
        public async Task<IActionResult> ChangePassword(string uuid, [FromBody] UserPasswordChangeDTO passwordChangeDto)
        {
            try
            {
                await _userManager.ChangePasswordAsync(uuid, passwordChangeDto.NewPassword, passwordChangeDto.OldPassword);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
