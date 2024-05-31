using BLL.Item_related;

namespace BLL.RepositoryInterfaces
{
    public interface IItemDAO
    {
        Task<Item> GetItemByIdAsync(int id);
        Task<IEnumerable<Item>> GetAllItemsAsync();
        Task CreateItemAsync(Item item);
        Task UpdateItemAsync(Item item);
        Task DeleteItemAsync(int id);
    }
}