using FinancialChatRoomApp.FinancialChatRoom.Configuration;
using FinancialChatRoomApp.FinancialChatRoom.Repositories;
using FinancialChatRoomApp.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinancialChatRoomAppTests.FinancialChatRoom.Repositories
{
    public class PostRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly PostRepository _postRepository;

        public PostRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _postRepository = new PostRepository(_context);
        }

        [Fact]
        public async Task InsertPostAsync_ShouldAddPost()
        {
            var post = new Post { UserName = "testUser", Message = "Test content", DateOfPost = DateTime.UtcNow };

            await _postRepository.InsertPostAsync(post);

            var result = await _postRepository.GetAllPostsAsync();
            Assert.Single(result);
            Assert.Equal("testUser", result.First().UserName);
            Assert.Equal("Test content", result.First().Message);
        }

        [Fact]
        public async Task DeletePostAsync_ShouldRemovePost_WhenPostExists()
        {
            var post = new Post { UserName = "testUser", Message = "Test content", DateOfPost = DateTime.UtcNow };

            await _postRepository.InsertPostAsync(post);

            await _postRepository.DeletePostAsync(1);

            var result = await _postRepository.GetAllPostsAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPostsAsync_ShouldReturnAllPosts()
        {
            var posts = new List<Post>
            {
                new Post { UserName = "testUser", Message = "Test content", DateOfPost = DateTime.UtcNow },
                new Post { UserName = "testUser1", Message = "Test content 1", DateOfPost = DateTime.UtcNow.AddMinutes(1) }
            };

            foreach (var post in posts)
            {
                await _postRepository.InsertPostAsync(post);
            }

            var result = await _postRepository.GetAllPostsAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.UserName == "testUser" && p.Message == "Test content");
            Assert.Contains(result, p => p.UserName == "testUser1" && p.Message == "Test content 1");
        }

        [Fact]
        public async Task GetLimitPostsAsync_ShouldReturnLimitedPostsInDescendingOrder()
        {
            var posts = new List<Post>
            {
                new Post { UserName = "testUser", Message = "Test content", DateOfPost = DateTime.UtcNow.AddHours(-2) },
                new Post { UserName = "testUser1", Message = "Test content 1", DateOfPost = DateTime.UtcNow.AddHours(-1) },
                new Post { UserName = "testUser", Message = "Test content 2", DateOfPost = DateTime.UtcNow }
            };

            foreach (var post in posts)
            {
                await _postRepository.InsertPostAsync(post);
            }

            var result = await _postRepository.GetLimitPostsAsync(2);

            Assert.Equal(2, result.Count);
            Assert.True(result.First().DateOfPost >= result.Last().DateOfPost);
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldReturnPost_WhenPostExists()
        {
            var post = new Post { UserName = "user1", Message = "Post 1", DateOfPost = DateTime.UtcNow };
            await _postRepository.InsertPostAsync(post);

            var result = await _postRepository.GetPostByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("user1", result.UserName);
        }

        [Fact]
        public async Task GetPostsFromUserAsync_ShouldReturnPostsOfUser()
        {
            var posts = new List<Post>
            {
                new Post { UserName = "user1", Message = "Post 1", DateOfPost = DateTime.UtcNow.AddHours(2) },
                new Post { UserName = "user1", Message = "Post 2", DateOfPost = DateTime.UtcNow.AddHours(1) },
                new Post { UserName = "user2", Message = "Post 3", DateOfPost = DateTime.UtcNow }
            };

            foreach (var post in posts)
            {
                await _postRepository.InsertPostAsync(post);
            }

            var result = await _postRepository.GetPostsFromUserAsync("user1");

            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("user1", p.UserName));
        }

        [Fact]
        public async Task UpdatePostAsync_ShouldUpdatePost()
        {
            var post = new Post { UserName = "user1", Message = "Old Content", DateOfPost = DateTime.UtcNow };
            await _postRepository.InsertPostAsync(post);

            post.Message = "Updated Content";

            await _postRepository.UpdatePostAsync(post);

            var result = await _postRepository.GetPostByIdAsync(1);

            Assert.Equal("Updated Content", result.Message);
        }
    }
}

