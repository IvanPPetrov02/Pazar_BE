using System.Security.Claims;
using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using BLL.DTOs;
using Role = BLL.Role;
using User = BLL.User;


namespace Pazar.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager _userManager;

    public UserController(UserManager userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("private-scoped")]
    [Authorize(Policy = "read:messages")]
    public IActionResult Scoped()
    {
        return Ok(new
        {
            Message = "Hello from a private endpoint! You need to be authenticated and have a scope of read:messages to see this."
        });
    }

    

    [HttpGet("{uuid}")]
    public async Task<IActionResult> GetUser(string uuid)
    {
        var user = await _userManager.GetUserByIdAsync(uuid);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("update/{uuid}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(string uuid, [FromBody] User updateUser)
    {
        try
        {
            updateUser.UUID = new Guid(uuid);

            //await _userManager.UpdateUserAsync(updateUser);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("delete/{uuid}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(string uuid)
    {
        try
        {
            await _userManager.DeleteUserAsync(uuid);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userManager.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO userdto)
    {
        var registerUser = new UserCreateRequest
        {
            Connection = "Username-Password-Authentication",
            Email = userdto.Email,
            Password = userdto.Password
        };
        try
        {
            //var userResponse = await _managementApiClient.Users.CreateAsync(registerUser);
            User user = new User(userdto.Email, userdto.Password);
            // await _userManager.CreateUserAsync(user);
            //return Ok(userResponse);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}
