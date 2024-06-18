using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Chat_related;
using BLL.RepositoryInterfaces;
using DAL.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ChatDAO : IChatDAO
    {
        private readonly AppDbContext _context;

        public ChatDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateChatAsync(Chat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");
            }

            try
            {
                _context.Chats.Add(chat);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the chat.", ex);
            }
        }

        public async Task<Chat> GetChatByIdAsync(int id)
        {
            try
            {
                var chat = await _context.Chats
                    .Include(c => c.ItemSold)
                    .ThenInclude(item => item.Seller)
                    .Include(c => c.Buyer)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (chat == null)
                {
                    throw new KeyNotFoundException($"Chat with ID {id} not found.");
                }

                return chat;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the chat.", ex);
            }
        }

        public async Task<IEnumerable<Chat>> GetAllChatsAsync()
        {
            try
            {
                var chats = await _context.Chats
                    .Include(c => c.ItemSold)
                    .ThenInclude(item => item.Seller)
                    .Include(c => c.Buyer)
                    .ToListAsync();

                if (chats == null || !chats.Any())
                {
                    throw new KeyNotFoundException("No chats found.");
                }

                return chats;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the chats.", ex);
            }
        }

        public async Task UpdateChatAsync(Chat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");
            }

            try
            {
                _context.Chats.Update(chat);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the chat.", ex);
            }
        }

        public async Task DeleteChatAsync(int id)
        {
            try
            {
                var chat = await GetChatByIdAsync(id);

                if (chat == null)
                {
                    throw new KeyNotFoundException($"Chat with ID {id} not found.");
                }

                _context.Chats.Remove(chat);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the chat.", ex);
            }
        }
    }
}
