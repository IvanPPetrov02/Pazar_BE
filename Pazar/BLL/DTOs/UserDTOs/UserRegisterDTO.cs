using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class UserRegisterDTO
{
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(50)]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    [MaxLength(90)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character.")]
    public string Password { get; set; }
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Surname is required")]
    public string Surname { get; set; }
}