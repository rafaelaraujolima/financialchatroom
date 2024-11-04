using FinancialChatRoomApp.FinancialChatRoom.Configuration;
using FinancialChatRoomApp.FinancialChatRoom.Dtos;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces.Repositories;
using FinancialChatRoomApp.FinancialChatRoom.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialChatRoomApp.FinancialChatRoom.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<User>();
        }

        public async Task DeleteAsync(string email)
        {
            User? user = await _dbSet
                .Where(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefaultAsync();

            if (user != null)
            {
                _dbSet.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IList<UserDto>> GetAllUsersAsync()
        {
            return await _dbSet
                .Select(user => new UserDto
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    CreatedAt = user.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            return await _dbSet
                .Where(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .Select(user => new UserDto
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    CreatedAt = user.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            return await _dbSet
                .Where(user => user.UserName.Equals(username, StringComparison.OrdinalIgnoreCase))
                .Select(user => new UserDto
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    CreatedAt = user.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(User user)
        {
            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _dbSet.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
