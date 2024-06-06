using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL;
using BLL.DTOs;
using System.Threading.Tasks;
using BLL.ManagerInterfaces;
using BLL.Services;
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
            else
            {
                return Unauthorized("Authentication failed");
            }
        }

        [Authorize]
        [HttpGet("{uuid}")]
        public async Task<IActionResult> GetUser(string uuid)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var user = await _userManager.GetUserByIdAsync(uuid);
            return user != null ? Ok(user) : NotFound();
        }

        [Authorize]
        [HttpPut("{uuid}")]
        public async Task<IActionResult> UpdateUser(string uuid, [FromBody] UserUpdateDTO userDto)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

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
        public async Task<IActionResult> DeleteUser(string uuid)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            try
            {
                await _userManager.DeleteUserAsync(uuid);
                return NoContent(); // Return 204
            }
            catch (InvalidOperationException ex)
            {
                // Return 404
                _logger.LogWarning("User not found for deletion: " + ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Return 500
                _logger.LogError(ex, "An error occurred while deleting the user");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetLoggedUser()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);
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
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var users = await _userManager.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpPost("activate/{uuid}")]
        public async Task<IActionResult> ActivateUser(string uuid)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            await _userManager.ActivateOrDeactivateUserAsync(uuid, true);
            return Ok();
        }

        [Authorize]
        [HttpPost("deactivate/{uuid}")]
        public async Task<IActionResult> DeactivateUser(string uuid)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            await _userManager.ActivateOrDeactivateUserAsync(uuid, false);
            return Ok();
        }

        [Authorize]
        [HttpPost("change-password/{uuid}")]
        public async Task<IActionResult> ChangePassword(string uuid, [FromBody] UserPasswordChangeDTO passwordChangeDto)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                _logger.LogWarning("Unauthorized access attempt with invalid or missing token");
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

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
