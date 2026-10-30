using FirstOrdersApi.Models;
using FirstOrdersApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstOrdersApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(DataStore store, TokenService tokens) : ControllerBase
{
    /// <summary>POST /api/auth/login: returns a JWT and the user.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        var user = store.ValidateCredentials(request.Username ?? "", request.Password ?? "");
        if (user is null)
        {
            return Unauthorized(new { message = "Incorrect username or password." });
        }
        return new LoginResponse(tokens.CreateToken(user), user);
    }

    /// <summary>GET /api/auth/me: the signed-in user (needs a valid token).</summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<User> Me()
    {
        var name = User.Identity?.Name;
        var user = store.Users.Values.FirstOrDefault(u => u.Username == name);
        return user is null ? Unauthorized() : user;
    }
}
