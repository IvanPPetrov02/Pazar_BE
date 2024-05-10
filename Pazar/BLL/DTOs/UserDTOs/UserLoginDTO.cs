using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class UserLoginDTO
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}