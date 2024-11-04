using FinancialChatRoomApp.FinancialChatRoom.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialChatRoomApp.FinancialChatRoom.Configuration.EntityMappings
{
    public class UserMapping : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Name)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.HasMany(u => u.Posts)
                .WithOne(p => p.User);

            builder.ToTable("Users");
        }
    }
}
