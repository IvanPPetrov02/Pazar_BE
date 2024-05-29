using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class UserUpdateDTO
{
    [MaxLength(50)]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public byte[]? Image { get; set; }
    public bool IsActive { get; set; }
    public Address? Address { get; set; }
}