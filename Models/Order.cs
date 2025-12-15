using System;
using System.Collections.Generic;

namespace BL5_PRN212_MustPass_Project.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Note { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual User CreatedByUser { get; set; } = null!;

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
