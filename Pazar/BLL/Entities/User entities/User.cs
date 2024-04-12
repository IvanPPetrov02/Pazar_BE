namespace BLL;

public class User
{
    public string UUID { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Phone { get; set; }
    public byte[] Image { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Role Role { get; set; }
    public Address Address { get; set; }

    public User(string uuid, string email, string name, string surname, string phone, byte[] image, bool isActive, DateTime createdAt, Role role, Address address)
    {
        UUID = uuid;
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
        Email = email;
        Password = password;
    }

    public User()
    {
        
    }
}