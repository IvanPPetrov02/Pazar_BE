using System.ComponentModel.DataAnnotations;

namespace BLL;

public class User
{
    [Key]
    public Guid UUID { get; set; }
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(50)]
    [EmailAddress]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    [MaxLength(90)]
    public string? Password { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
    [MaxLength(50)]
    public string Surname { get; set; }

    public byte[]? Image { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Role Role { get; set; } = Role.User;
    public Address? Address { get; set; }

    public User( string email, string name, string surname, byte[]? image, bool isActive, DateTime createdAt, Role role, Address? address)
    {
        UUID = Guid.NewGuid();
        Email = email;
        Name = name;
        Surname = surname;
        Image = image;
        IsActive = isActive;
        CreatedAt = createdAt;
        Role = role;
        Address = address;
    }

    public User(string email, string? password)
    {
        Email = email;
        Password = password;
    }

    public User()
    {
        
    }
}