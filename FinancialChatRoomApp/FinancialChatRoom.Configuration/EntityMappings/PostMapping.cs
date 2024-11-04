using FinancialChatRoomApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialChatRoomApp.FinancialChatRoom.Configuration.EntityMappings
{
    public class PostMapping : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(p => p.Message)
                .IsRequired();

            builder.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserName)
                .HasPrincipalKey(u => u.UserName)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(p => p.DateOfPost)
                .IsRequired();

            builder.ToTable("posts");
        }
    }
}
