
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ShopException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class CartService(AppDbContext db)
{
    private async Task<int> GetActiveCartIdAsync()
    {
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.Status == "Active");
        if (cart == null)
        {
            cart = new Cart();
            db.Carts.Add(cart);
            await db.SaveChangesAsync();
        }
        return cart.CartId;
    }

    public async Task<CartDto> GetCartAsync()
    {
        var cartId = await GetActiveCartIdAsync();

        var items = await (
            from i in db.CartItems
            join p in db.Products on i.ProductId equals p.ProductId
            where i.CartId == cartId
            orderby p.ProductCode
            select new CartItemDto(p.ProductId, p.ProductCode, p.ProductName, p.UnitPrice, i.Quantity, p.UnitPrice * i.Quantity)
        ).ToListAsync();

        return new CartDto(cartId, items, items.Sum(i => i.LineTotal));
    }

    public async Task<CartDto> AddAsync(int productId, int quantity)
    {
        if (quantity <= 0) throw new ShopException(400, "จำนวนต้องมากกว่า 0");

        var cartId = await GetActiveCartIdAsync();
        var current = await db.CartItems
            .Where(i => i.CartId == cartId && i.ProductId == productId)
            .Select(i => i.Quantity)
            .FirstOrDefaultAsync();

        return await SetQuantityAsync(productId, current + quantity);
    }
     
    public async Task<CartDto> SetQuantityAsync(int productId, int newQuantity)
    {
        if (newQuantity < 0) throw new ShopException(400, "จำนวนต้องไม่ติดลบ");

        await using var transaction = await db.Database.BeginTransactionAsync();

        var cartId = await GetActiveCartIdAsync();
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);
        var delta = newQuantity - (item?.Quantity ?? 0);

        if (delta > 0)
        {
            var reserved = await db.Stocks
                .Where(s => s.ProductId == productId && s.Quantity >= delta)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Quantity, x => x.Quantity - delta));

            if (reserved == 0)
            {
                var productExists = await db.Products.AnyAsync(p => p.ProductId == productId);
                throw productExists
                    ? new ShopException(409, "สินค้าคงเหลือไม่พอ")
                    : new ShopException(404, "ไม่พบสินค้า");
            }
        }
        else if (delta < 0)
        {
            var returned = -delta;
            await db.Stocks
                .Where(s => s.ProductId == productId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Quantity, x => x.Quantity + returned));
        }

        if (item == null && newQuantity > 0)
            db.CartItems.Add(new CartItem { CartId = cartId, ProductId = productId, Quantity = newQuantity });
        else if (item != null && newQuantity == 0)
            db.CartItems.Remove(item);
        else if (item != null)
            item.Quantity = newQuantity;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetCartAsync();
    }

    public async Task<CartDto> ClearAsync()
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var cartId = await GetActiveCartIdAsync();
        var items = await db.CartItems.Where(i => i.CartId == cartId).ToListAsync();

        foreach (var item in items)
        {
            var productId = item.ProductId;
            var quantity = item.Quantity;
            await db.Stocks
                .Where(s => s.ProductId == productId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Quantity, x => x.Quantity + quantity));
        }

        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetCartAsync();
    }

    public async Task<OrderDto> CheckoutAsync()
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var cartId = await db.Carts
            .Where(c => c.Status == "Active")
            .Select(c => (int?)c.CartId)
            .FirstOrDefaultAsync()
            ?? throw new ShopException(400, "ตะกร้าว่าง");

        var closed = await db.Carts
            .Where(c => c.CartId == cartId && c.Status == "Active")
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, "CheckedOut"));
        if (closed == 0) throw new ShopException(409, "ตะกร้านี้ Checkout ไปแล้ว");

        var lines = await (
            from i in db.CartItems
            join p in db.Products on i.ProductId equals p.ProductId
            where i.CartId == cartId
            orderby p.ProductCode
            select new CartItemDto(p.ProductId, p.ProductCode, p.ProductName, p.UnitPrice, i.Quantity, p.UnitPrice * i.Quantity)
        ).ToListAsync();
        if (lines.Count == 0) throw new ShopException(400, "ตะกร้าว่าง");

        var order = new Order { CartId = cartId, TotalAmount = lines.Sum(l => l.LineTotal) };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        db.OrderItems.AddRange(lines.Select(l => new OrderItem
        {
            OrderId = order.OrderId,
            ProductId = l.ProductId,
            ProductCode = l.ProductCode,
            ProductName = l.ProductName,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity
        }));
        await db.SaveChangesAsync();

        aw ait db.CartItems.Where(i => i.CartId == cartId).ExecuteDeleteAsync();

        await transaction.CommitAsync();
        return new OrderDto(order.OrderId, order.TotalAmount, lines);
    }
}
