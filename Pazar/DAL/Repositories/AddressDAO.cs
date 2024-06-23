using BLL;
using BLL.RepositoryInterfaces;
using DAL.DbContexts;

namespace DAL.Repositories
{
    public class AddressDAO : IAddressDAO
    {
        private readonly AppDbContext _context;

        public AddressDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAddressAsync(Address address)
        {
            _context.Address.Add(address);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAddressAsync(Address address)
        {
            _context.Address.Update(address);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAddressAsync(int id)
        {
            var address = await _context.Address.FindAsync(id);
            if (address != null)
            {
                _context.Address.Remove(address);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Address?> GetAddressByIdAsync(int id)
        {
            return id != 0 ? await _context.Address.FindAsync(id) : null;
        }
    }
}