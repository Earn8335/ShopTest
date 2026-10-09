using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
