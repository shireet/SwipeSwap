using Microsoft.EntityFrameworkCore;
using SwipeSwap.Domain.Models;

namespace SwipeSwap.Infrastructure.Postgres.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ItemTag> ItemTags => Set<ItemTag>();
    public DbSet<Barter> Barters => Set<Barter>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Exchange> Exchanges => Set<Exchange>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}