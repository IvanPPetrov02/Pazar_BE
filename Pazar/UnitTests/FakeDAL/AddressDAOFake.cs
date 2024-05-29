using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using BLL.RepositoryInterfaces;

namespace UnitTests.FakeDAL
{
    public class AddressDAOFake : IAddressDAO
    {
        private readonly List<Address> _addresses;

        public AddressDAOFake()
        {
            _addresses = new List<Address>
            {
                new Address
                {
                    ID = 1,
                    Country = "Country1",
                    City = "City1",
                    Street = "Street1",
                    Number = "Number1",
                    ZipCode = "ZipCode1"
                },
                new Address
                {
                    ID = 2,
                    Country = "Country2",
                    City = "City2",
                    Street = "Street2",
                    Number = "Number2",
                    ZipCode = "ZipCode2"
                },
                new Address
                {
                    ID = 3,
                    Country = "Country3",
                    City = "City3",
                    Street = "Street3",
                    Number = "Number3",
                    ZipCode = "ZipCode3"
                }
            };
        }

        public Task CreateAddressAsync(Address address)
        {
            _addresses.Add(address);
            return Task.CompletedTask;
        }

        public Task UpdateAddressAsync(Address address)
        {
            var existingAddress = _addresses.FirstOrDefault(a => a.ID == address.ID);
            if (existingAddress != null)
            {
                _addresses.Remove(existingAddress);
                _addresses.Add(address);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAddressAsync(int id)
        {
            var address = _addresses.FirstOrDefault(a => a.ID == id);
            if (address != null)
            {
                _addresses.Remove(address);
            }
            return Task.CompletedTask;
        }

        public Task<Address?> GetAddressByIdAsync(int id)
        {
            var address = _addresses.FirstOrDefault(a => a.ID == id);
            return Task.FromResult(address);
        }

    }
}
