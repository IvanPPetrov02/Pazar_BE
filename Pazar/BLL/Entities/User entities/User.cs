using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLL;

public class User
{
    [Key]
    public  Guid UUID { get; set; }
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(50)]
    [EmailAddress]
    public  string Email { get; set; }
    [Required(ErrorMessage = "Password is required")]
    [MaxLength(50)]
    public string? Password { get; set; }
    public  string Name { get; set; }
    public  string Surname { get; set; }
    public  string Phone { get; set; }
    public byte[]? Image { get; set; }
    public  bool IsActive { get; set; }
    public  DateTime CreatedAt { get; set; }
    public  Role Role { get; set; }
    public  Address Address { get; set; }

    public User( string email, string name, string surname, string phone, byte[] image, bool isActive, DateTime createdAt, Role role, Address address)
    {
        UUID = Guid.NewGuid();
        Email = email;
        Name = name;
        Surname = surname;
        Phone = phone;
        Image = image;
        IsActive = isActive;
        CreatedAt = createdAt;
        Role = role;
        Address = address;
    }

    public User(string email, string? password)
    {
        UUID = Guid.NewGuid();
        Email = email;
        Password = password;
        Name = "null";
        Surname = "null";
        Phone = "null";
        Image = null;
        IsActive = true;
        CreatedAt = DateTime.Now;
        Role = Role.User;
        Address = new Address();
    }

    public User()
    {
        
    }
}