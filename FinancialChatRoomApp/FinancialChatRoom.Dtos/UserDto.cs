using FinancialChatRoomApp.Models;

namespace FinancialChatRoomApp.FinancialChatRoom.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<Post> Posts { get; set; }
    }
}
