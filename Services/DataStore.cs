using System.Collections.Concurrent;
using FirstOrdersApi.Models;

namespace FirstOrdersApi.Services;

/// <summary>
/// In-memory data for the demo. Registered as a singleton, so data resets when the app restarts.
/// Replace with EF Core / a real database for production.
/// </summary>
public class DataStore
{
    // Demo credentials only. Never store plain-text passwords in a real app.
    private readonly Dictionary<string, string> _passwords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = "admin123",
        ["manager"] = "manager123",
        ["customer"] = "customer123",
    };

    public ConcurrentDictionary<int, User> Users { get; } = new(new Dictionary<int, User>
    {
        [1] = new(1, "admin", "Alice Admin", Roles.Admin),
        [2] = new(2, "manager", "Mark Manager", Roles.Manager),
        [3] = new(3, "customer", "Cara Customer", Roles.Customer),
    });

    public ConcurrentDictionary<int, Product> Products { get; } = new(new Dictionary<int, Product>
    {
        [1] = new(1, "Ergonomic Keyboard", "Accessories", 89.99m, 42),
        [2] = new(2, "27-inch Monitor", "Displays", 329.00m, 15),
        [3] = new(3, "Noise-Cancelling Headphones", "Audio", 199.50m, 8),
        [4] = new(4, "USB-C Dock", "Accessories", 119.00m, 0),
        [5] = new(5, "Webcam HD", "Video", 59.99m, 27),
        [6] = new(6, "Studio Microphone", "Audio", 149.00m, 11),
    });

    public ConcurrentDictionary<int, Order> Orders { get; } = new(new Dictionary<int, Order>
    {
        [1001] = new(1001, "Acme Corp", "2026-10-20", 419.99m, "Shipped"),
        [1002] = new(1002, "Globex Ltd", "2026-10-22", 89.99m, "Processing"),
        [1003] = new(1003, "Initech", "2026-10-24", 658.00m, "Delivered"),
        [1004] = new(1004, "Umbrella Inc", "2026-10-26", 149.00m, "Cancelled"),
        [1005] = new(1005, "Stark Industries", "2026-10-27", 328.49m, "Processing"),
    });

    private int _productId = 6;
    private int _orderId = 1005;

    public int NextProductId() => Interlocked.Increment(ref _productId);
    public int NextOrderId() => Interlocked.Increment(ref _orderId);

    public User? ValidateCredentials(string username, string password)
    {
        if (!_passwords.TryGetValue(username.Trim(), out var expected) || expected != password)
        {
            return null;
        }
        return Users.Values.FirstOrDefault(u => u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
