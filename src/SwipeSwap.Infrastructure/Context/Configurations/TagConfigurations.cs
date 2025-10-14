using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Context.Configurations;

public class TagConfigurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("tags");
            
            builder.HasKey(t => t.Id);
            
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.HasIndex(t => t.Name).IsUnique();
        }
    }
}