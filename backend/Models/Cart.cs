using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Order? Order { get; set; }
}
