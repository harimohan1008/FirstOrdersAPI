using FirstOrdersApi.Models;
using FirstOrdersApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstOrdersApi.Controllers;

/// <summary>Any signed-in user can read; managers and admins can write; only admins can delete.</summary>
[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController(DataStore store) : ControllerBase
{
    // GET /api/products?search=key&category=Audio
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll([FromQuery] string? search, [FromQuery] string? category)
    {
        IEnumerable<Product> items = store.Products.Values.OrderBy(p => p.Id);
        if (!string.IsNullOrWhiteSpace(search))
            items = items.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(category))
            items = items.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        return Ok(items);
    }

    // GET /api/products/3
    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id) =>
        store.Products.TryGetValue(id, out var product) ? product : NotFound();

    // POST /api/products
    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public ActionResult<Product> Create(ProductInput input)
    {
        if (!Validate(input, out var error)) return BadRequest(new { message = error });

        var product = new Product(store.NextProductId(), input.Name.Trim(), input.Category.Trim(), input.Price, input.Stock);
        store.Products[product.Id] = product;
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT /api/products/3
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public ActionResult<Product> Update(int id, ProductInput input)
    {
        if (!store.Products.ContainsKey(id)) return NotFound();
        if (!Validate(input, out var error)) return BadRequest(new { message = error });

        var updated = new Product(id, input.Name.Trim(), input.Category.Trim(), input.Price, input.Stock);
        store.Products[id] = updated;
        return updated;
    }

    // DELETE /api/products/3
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult Delete(int id) =>
        store.Products.TryRemove(id, out _) ? NoContent() : NotFound();

    private static bool Validate(ProductInput input, out string error)
    {
        error = "";
        if (string.IsNullOrWhiteSpace(input.Name)) error = "Name is required.";
        else if (string.IsNullOrWhiteSpace(input.Category)) error = "Category is required.";
        else if (input.Price < 0) error = "Price cannot be negative.";
        else if (input.Stock < 0) error = "Stock cannot be negative.";
        return error == "";
    }
}
