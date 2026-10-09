using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<List<ProductDto>> GetAll()
    {
        return await (
            from p in db.Products
            join s in db.Stocks on p.ProductId equals s.ProductId
            orderby p.ProductCode
            select new ProductDto(p.ProductId, p.ProductCode, p.ProductName, p.UnitPrice, p.Description, s.Quantity)
        ).ToListAsync();
    }
}

[ApiController]
[Route("api/cart")]
public class CartController(CartService service) : ControllerBase
{
    [HttpGet]
    public Task<CartDto> Get() => service.GetCartAsync();

    [HttpPost("items")]
    public Task<CartDto> Add(AddItemRequest request) => service.AddAsync(request.ProductId, request.Quantity);

    [HttpPut("items/{productId:int}")]
    public Task<CartDto> SetQuantity(int productId, SetQuantityRequest request) => service.SetQuantityAsync(productId, request.Quantity);

    [HttpDelete("items/{productId:int}")]
    public Task<CartDto> Remove(int productId) => service.SetQuantityAsync(productId, 0);

    [HttpDelete]
    public Task<CartDto> Clear() => service.ClearAsync();
}

[ApiController]
[Route("api/checkout")]
public class CheckoutController(CartService service) : ControllerBase
{
    [HttpPost]
    public Task<OrderDto> Checkout() => service.CheckoutAsync();
}
