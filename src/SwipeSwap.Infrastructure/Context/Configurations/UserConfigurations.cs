using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Context.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.EncryptedSensitiveData)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasMany(typeof(Item))
            .WithOne()
            .HasForeignKey("OwnerId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(u => u.Rating);
        
        builder.HasMany(typeof(Review))
            .WithOne()
            .HasForeignKey("ToUserId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}