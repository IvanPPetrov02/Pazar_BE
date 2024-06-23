using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class UserPasswordChangeDTO
{
    [Required(ErrorMessage = "Old Password is required")]
    public string OldPassword { get; set; }

    [Required(ErrorMessage = "New Password is required")]
    [MaxLength(90)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>+-]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one number, and one special character from !@#$%^&*(),.?\":{}|<>+-")]
    public string NewPassword { get; set; }
}