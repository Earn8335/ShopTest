using System.Collections.Generic;

namespace Backend;

public record ProductDto(int ProductId, string ProductCode, string ProductName, decimal UnitPrice, string? Description, int StockQuantity);
public record CartItemDto(int ProductId, string ProductCode, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public record CartDto(int CartId, List<CartItemDto> Items, decimal TotalAmount);
public record OrderDto(int OrderId, decimal TotalAmount, List<CartItemDto> Items);

public record AddItemRequest(int ProductId, int Quantity);
public record SetQuantityRequest(int Quantity);