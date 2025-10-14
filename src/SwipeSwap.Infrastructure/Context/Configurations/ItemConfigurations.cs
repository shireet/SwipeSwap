using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Context.Configurations;

public class ItemConfigurations : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(i => i.ItemTags)
            .WithOne(t => t.Item)
            .HasForeignKey(t => t.ItemId);
    }
}