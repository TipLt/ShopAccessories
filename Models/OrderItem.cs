using System;
using System.Collections.Generic;

namespace BL5_PRN212_MustPass_Project.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
