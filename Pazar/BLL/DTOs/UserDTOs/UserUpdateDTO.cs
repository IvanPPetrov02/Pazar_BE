namespace BLL.DTOs;

public class UserUpdateDTO
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public byte[]? Image { get; set; }
    public bool IsActive { get; set; }
    public Address? Address { get; set; }
}