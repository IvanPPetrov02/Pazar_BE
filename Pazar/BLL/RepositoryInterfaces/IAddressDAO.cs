namespace BLL.RepositoryInterfaces;

public interface IAddressDAO
{
    Task CreateAddressAsync(Address address);
    Task UpdateAddressAsync(Address address);
    Task DeleteAddressAsync(int id);
    Task<Address>? GetAddressByIdAsync(int id);
}