using FinancialChatRoomApp.FinancialChatRoom.Configuration.EntityMappings;
using FinancialChatRoomApp.FinancialChatRoom.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinancialChatRoomApp.FinancialChatRoom.Configuration
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfiguration(new UserMapping());
            modelBuilder.ApplyConfiguration(new PostMapping());
            modelBuilder.Ignore<CommandResult>();
            modelBuilder.Ignore<SendMessage>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
