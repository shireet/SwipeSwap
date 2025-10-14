namespace SwipeSwap.Domain.Models;

public class User :  BaseEntity
{
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string EncryptedSensitiveData { get; set; }
    public double Rating { get; set; } = 0;
    public List<Item> Items = [];
    public List<Review> Reviews = [];
    
    private User() {}
}