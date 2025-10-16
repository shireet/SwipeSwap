using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Postgres.Context.Configurations;

public class ItemTagConfigurations
{
    public class ItemTagConfiguration : IEntityTypeConfiguration<ItemTag>
    {
        public void Configure(EntityTypeBuilder<ItemTag> builder)
        {
            builder.ToTable("item_tags");
            builder.HasKey(it => new { it.ItemId, it.TagId });
        }
    }
}