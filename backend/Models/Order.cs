using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int CartId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
