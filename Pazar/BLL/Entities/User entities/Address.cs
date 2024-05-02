using System.ComponentModel.DataAnnotations;

namespace BLL;

public class Address
{
    [Key]
    public int ID { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string Number { get; set; }
    public string ZipCode { get; set; }
}