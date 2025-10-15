namespace SwipeSwap.Domain.Models;

public class User :  BaseEntity
{
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string EncryptedSensitiveData { get; set; }
    public double Rating { get; set; } = 0;
    public List<Item> Items = [];
    public List<Review> Reviews = [];
    
    public User() {}
}