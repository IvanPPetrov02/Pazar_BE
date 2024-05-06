using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BLL;
using BLL.DTOs;
using System.Threading.Tasks;
using BLL.ManagerInterfaces;
using BLL.Services;

namespace Pazar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly IJwtService _jwtService;

    public UserController(IUserManager userManager, IJwtService? jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO userDto)
    {
        var result = await _userManager.RegisterUserAsync(userDto);
        if (result == "User created")
        {
            return Ok(new { message = result });
        }
        else
        {
            return BadRequest(new { message = result });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO loginDto)
    {
        var token = await _userManager.AuthenticateUserAsync(loginDto.Email, loginDto.Password);
        if (token != null)
        {
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
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet]
    [Route("GetUser")]
    public IActionResult GetLoggedUser()    
    {
        try
        {
            var jwt = Request.Cookies["jwt"];

            var token = _jwtService.ValidateToken(jwt);

            string userID = token.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = GetUser(userID);

            return Ok(user);
        }
        catch(Exception _)
        {
            return Unauthorized();
        }
    }

    [Authorize]
    [HttpDelete("{uuid}")]
    public async Task<IActionResult> DeleteUser(string uuid)
    {
        await _userManager.DeleteUserAsync(uuid);
        return NoContent();
    }

    [Authorize]
    [HttpGet]
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
        return NoContent();
    }

    [Authorize]
    [HttpPost("deactivate/{uuid}")]
    public async Task<IActionResult> DeactivateUser(string uuid)
    {
        await _userManager.ActivateOrDeactivateUserAsync(uuid, false);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password/{uuid}")]
    public async Task<IActionResult> ChangePassword(string uuid, [FromBody] UserPasswordChangeDTO passwordChangeDto)
    {
        try
        {
            await _userManager.ChangePasswordAsync(uuid, passwordChangeDto.NewPassword, passwordChangeDto.OldPassword);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
