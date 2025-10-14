using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Context.Configurations;

public class MessageConfigurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("messages");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Text)
                .IsRequired()
                .HasMaxLength(1000);
            builder.Property(m => m.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}