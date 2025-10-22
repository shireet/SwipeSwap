using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

public class ExchangeConfiguration : IEntityTypeConfiguration<Exchange>
{
    public void Configure(EntityTypeBuilder<Exchange> b)
    {
        b.ToTable("exchanges");
        b.HasKey(x => x.Id);

        b.Property(x => x.InitiatorId).IsRequired();
        b.Property(x => x.ReceiverId).IsRequired();
        b.Property(x => x.OfferedItemId).IsRequired();
        b.Property(x => x.RequestedItemId).IsRequired();
        b.Property(x => x.Status).HasConversion<int>().IsRequired();
        b.Property(x => x.Message).HasMaxLength(1000);

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);

        b.HasIndex(x => new { x.InitiatorId, x.Status });
        b.HasIndex(x => new { x.ReceiverId, x.Status });
    }
}