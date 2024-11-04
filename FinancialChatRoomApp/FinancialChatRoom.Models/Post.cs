using FinancialChatRoomApp.FinancialChatRoom.Models;

namespace FinancialChatRoomApp.Models
{
    public class Post
    {
        public int Id { get; set; }
        public DateTime DateOfPost { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public User User { get; set; }
    }
}
