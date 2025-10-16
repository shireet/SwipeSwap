using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Postgres.Context.Configurations;

public class ChatConfigurations
{
    public class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("chats");
            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.Messages)
                .WithOne()
                .HasForeignKey("ChatId");
        }
    }
}