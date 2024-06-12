using BLL.DTOs.ItemDTOs;
using BLL.Item_related;

namespace BLL.ManagerInterfaces
{
    public interface IItemManager
    {
        Task CreateItemAsync(ItemCreateDTO itemDto);
        Task UpdateItemAsync(ItemUpdateDTO itemDto);
        Task DeleteItemAsync(int id);
        Task<Item?> GetItemByIdAsync(int id);
        Task<IEnumerable<Item>> GetAllItemsAsync();
        Task UpdateItemStatusAsync(int id, ItemStatus status);
        Task UpdateItemImagesAsync(int id, List<ItemImages> images);
        Task<bool> IsUserSellerAsync(int itemId, string userId);
    }
}