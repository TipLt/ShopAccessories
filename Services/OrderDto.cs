using BL5_PRN212_MustPass_Project.Models;
using System.Linq;

namespace BL5_PRN212_MustPass_Project.Services;

public class OrderDto
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUsername { get; set; } = null!;
    public List<OrderItemDto> Items { get; set; } = new();
    
    // Calculated property: Total quantity across all items
    public int TotalQuantity => Items?.Sum(i => i.Quantity) ?? 0;
}

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string ProductCode { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
