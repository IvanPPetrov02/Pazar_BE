using BLL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // For returning a list of users

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
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        try
        {
            await _userManager.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUser), new { uuid = user.UUID }, user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
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

    [HttpPut("{uuid}")]
    public async Task<IActionResult> UpdateUser(string uuid, [FromBody] User updateUser)
    {
        try
        {
            updateUser.UUID = uuid;

            await _userManager.UpdateUserAsync(updateUser);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpDelete("{uuid}")]
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
}
