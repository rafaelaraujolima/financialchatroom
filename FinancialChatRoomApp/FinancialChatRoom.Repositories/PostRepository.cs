using FinancialChatRoomApp.FinancialChatRoom.Configuration;
using FinancialChatRoomApp.FinancialChatRoom.Interfaces.Repositories;
using FinancialChatRoomApp.FinancialChatRoom.Models;
using FinancialChatRoomApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialChatRoomApp.FinancialChatRoom.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Post> _dbSet;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Post>();
        }

        public async Task DeletePostAsync(int postId)
        {
            Post? post = await _dbSet
                .Where(p => p.Id == postId)
                .SingleOrDefaultAsync();

            if (post != null)
            {
                _dbSet.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IList<Post>> GetAllPostsAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IList<Post>> GetLimitPostsAsync(int limit)
        {
            return await _dbSet.OrderByDescending(p => p.DateOfPost).Take(limit).ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            Post? post = await _dbSet.FindAsync(postId);

            return post;
        }

        public async Task<IList<Post>> GetPostsFromUserAsync(string userName)
        {
            List<Post> posts = await _dbSet
                .Where(p => p.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            return posts;
        }

        public async Task InsertPostAsync(Post post)
        {
            await _dbSet.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _dbSet.Update(post);
            await _context.SaveChangesAsync();
        }
    }
}
