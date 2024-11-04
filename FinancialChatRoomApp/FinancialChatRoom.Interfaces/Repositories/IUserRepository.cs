using FinancialChatRoomApp.FinancialChatRoom.Dtos;
using FinancialChatRoomApp.FinancialChatRoom.Models;

namespace FinancialChatRoomApp.FinancialChatRoom.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<IList<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task InsertAsync(User user);
        Task DeleteAsync(string email);
        Task UpdateAsync(User user);        
    }
}
