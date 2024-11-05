using FinancialChatRoomApp.Models;

namespace FinancialChatRoomApp.FinancialChatRoom.Interfaces.Repositories
{
    public interface IPostRepository
    {
        Task<IList<Post>> GetAllPostsAsync();
        Task<IList<Post>> GetLimitPostsAsync(int limit);
        Task<Post?> GetPostByIdAsync(int postId);
        Task<IList<Post>> GetPostsFromUserAsync(string userName);
        Task InsertPostAsync(Post post);
        Task UpdatePostAsync(Post post);
        Task DeletePostAsync(int postId);
    }
}
