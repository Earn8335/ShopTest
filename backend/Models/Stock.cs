using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Stock
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Product Product { get; set; } = null!;
}
