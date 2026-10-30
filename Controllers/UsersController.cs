using FirstOrdersApi.Models;
using FirstOrdersApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstOrdersApi.Controllers;

/// <summary>Used by the Angular Admin page. Admins only.</summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController(DataStore store) : ControllerBase
{
    // GET /api/users
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetAll() => Ok(store.Users.Values.OrderBy(u => u.Id));

    // GET /api/users/2
    [HttpGet("{id:int}")]
    public ActionResult<User> GetById(int id) =>
        store.Users.TryGetValue(id, out var user) ? user : NotFound();

    // PUT /api/users/2/role   body: { "role": "admin" }
    [HttpPut("{id:int}/role")]
    public ActionResult<User> UpdateRole(int id, RoleUpdate input)
    {
        if (!store.Users.TryGetValue(id, out var user)) return NotFound();
        if (!Roles.All.Contains(input.Role, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = $"Role must be one of: {string.Join(", ", Roles.All)}." });

        var updated = user with { Role = input.Role.ToLowerInvariant() };
        store.Users[id] = updated;
        return updated;
    }

    // DELETE /api/users/3
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (User.Identity?.Name is { } me && store.Users.TryGetValue(id, out var target) && target.Username == me)
            return BadRequest(new { message = "You cannot delete your own account." });

        return store.Users.TryRemove(id, out _) ? NoContent() : NotFound();
    }
}
