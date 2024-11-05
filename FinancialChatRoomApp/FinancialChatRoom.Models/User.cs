using FinancialChatRoomApp.Models;
using Microsoft.AspNetCore.Identity;

namespace FinancialChatRoomApp.FinancialChatRoom.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public IList<Post> Posts { get; set; }
    }
}
