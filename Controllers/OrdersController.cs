using FirstOrdersApi.Models;
using FirstOrdersApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstOrdersApi.Controllers;

/// <summary>Matches the Angular route guard: managers and admins only. Only admins can delete.</summary>
[ApiController]
[Route("api/orders")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
public class OrdersController(DataStore store) : ControllerBase
{
    private static readonly string[] Statuses = ["Processing", "Shipped", "Delivered", "Cancelled"];

    // GET /api/orders?status=Shipped
    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetAll([FromQuery] string? status)
    {
        IEnumerable<Order> items = store.Orders.Values.OrderBy(o => o.Id);
        if (!string.IsNullOrWhiteSpace(status))
            items = items.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        return Ok(items);
    }

    // GET /api/orders/1001
    [HttpGet("{id:int}")]
    public ActionResult<Order> GetById(int id) =>
        store.Orders.TryGetValue(id, out var order) ? order : NotFound();

    // POST /api/orders
    [HttpPost]
    public ActionResult<Order> Create(OrderInput input)
    {
        if (!Validate(input, out var error)) return BadRequest(new { message = error });

        var order = new Order(store.NextOrderId(), input.Customer.Trim(), input.Date, input.Total, Normalize(input.Status));
        store.Orders[order.Id] = order;
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // PUT /api/orders/1001
    [HttpPut("{id:int}")]
    public ActionResult<Order> Update(int id, OrderInput input)
    {
        if (!store.Orders.ContainsKey(id)) return NotFound();
        if (!Validate(input, out var error)) return BadRequest(new { message = error });

        var updated = new Order(id, input.Customer.Trim(), input.Date, input.Total, Normalize(input.Status));
        store.Orders[id] = updated;
        return updated;
    }

    // DELETE /api/orders/1001
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Delete(int id) =>
        store.Orders.TryRemove(id, out _) ? NoContent() : NotFound();

    private static string Normalize(string status) =>
        Statuses.First(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));

    private static bool Validate(OrderInput input, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(input.Customer)) error = "Customer is required.";
        else if (!DateOnly.TryParse(input.Date, out _)) error = "Date must be a valid date (yyyy-MM-dd).";
        else if (input.Total < 0) error = "Total cannot be negative.";
        else if (!Statuses.Contains(input.Status, StringComparer.OrdinalIgnoreCase))
            error = $"Status must be one of: {string.Join(", ", Statuses)}.";
        return error == "";
    }
}
