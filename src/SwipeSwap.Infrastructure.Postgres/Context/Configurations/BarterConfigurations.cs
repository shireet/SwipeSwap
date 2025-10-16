using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Postgres.Context.Configurations;

public class BarterConfigurations
{
    public class BarterConfiguration : IEntityTypeConfiguration<Barter>
    {
        public void Configure(EntityTypeBuilder<Barter> builder)
        {
            builder.ToTable("barters");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(b => b.ChatId).IsRequired(true);
        }
    }
}