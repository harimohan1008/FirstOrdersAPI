namespace FirstOrdersApi.Models;

public static class Roles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Customer = "customer";

    public static readonly string[] All = [Admin, Manager, Customer];
}

// Shapes match the Angular models in src/app/core/models.ts (camelCase JSON by default).
public record User(int Id, string Username, string DisplayName, string Role);

public record Product(int Id, string Name, string Category, decimal Price, int Stock);

public record Order(int Id, string Customer, string Date, decimal Total, string Status);

// Request bodies (no Id: the server assigns it)
public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, User User);
public record ProductInput(string Name, string Category, decimal Price, int Stock);
public record OrderInput(string Customer, string Date, decimal Total, string Status);
public record RoleUpdate(string Role);
