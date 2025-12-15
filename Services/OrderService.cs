using BL5_PRN212_MustPass_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace BL5_PRN212_MustPass_Project.Services;

public class OrderService : IOrderService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthService _authService;

    public OrderService(IAuthorizationService authorizationService, IAuthService authService)
    {
        _authorizationService = authorizationService;
        _authService = authService;
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        _authorizationService.EnsureCanRead("Orders");
        using var context = new ShopAccessoriesContext();

        var orders = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderDto>> GetOrdersByCustomerAsync(int customerId)
    {
        _authorizationService.EnsureCanRead("Orders");
        _authorizationService.EnsureCustomerSelf(customerId);

        using var context = new ShopAccessoriesContext();
        var orders = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        _authorizationService.EnsureCanRead("Orders");
        using var context = new ShopAccessoriesContext();

        var order = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return null;

        if (_authorizationService.IsCustomer())
        {
            _authorizationService.EnsureCustomerSelf(order.CustomerId ?? 0);
        }

        return MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(OrderDto orderDto)
    {
        _authorizationService.EnsureCanCreate("Orders");

        if (orderDto.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item");

        using var context = new ShopAccessoriesContext();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in orderDto.Items)
            {
                if (item.Quantity <= 0)
                    throw new InvalidOperationException($"Quantity must be greater than 0 for product {item.ProductName}");

                var product = await context.Products.FindAsync(item.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product {item.ProductName} not found");

                if (product.Quantity < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.Quantity}, Required: {item.Quantity}");
            }

            var order = new Order
            {
                CustomerId = orderDto.CustomerId,
                CreatedByUserId = _authService.CurrentUser!.UserId,
                CreatedAt = DateTime.UtcNow,
                Note = orderDto.Note,
                TotalAmount = orderDto.Items.Sum(i => i.LineTotal)
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            foreach (var itemDto in orderDto.Items)
            {
                var product = await context.Products.FindAsync(itemDto.ProductId);
                product!.Quantity -= itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = itemDto.ProductId,
                    UnitPrice = itemDto.UnitPrice,
                    Quantity = itemDto.Quantity,
                    LineTotal = itemDto.LineTotal
                };

                context.OrderItems.Add(orderItem);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetByIdAsync(order.OrderId))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(OrderDto orderDto)
    {
        _authorizationService.EnsureCanUpdate("Orders");

        if (orderDto.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item");

        using var context = new ShopAccessoriesContext();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderDto.OrderId);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            foreach (var oldItem in order.OrderItems)
            {
                var product = await context.Products.FindAsync(oldItem.ProductId);
                if (product != null)
                {
                    product.Quantity += oldItem.Quantity;
                }
            }

            context.OrderItems.RemoveRange(order.OrderItems);
            await context.SaveChangesAsync();

            foreach (var item in orderDto.Items)
            {
                if (item.Quantity <= 0)
                    throw new InvalidOperationException($"Quantity must be greater than 0 for product {item.ProductName}");

                var product = await context.Products.FindAsync(item.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product {item.ProductName} not found");

                if (product.Quantity < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.Quantity}, Required: {item.Quantity}");
            }

            order.CustomerId = orderDto.CustomerId;
            order.Note = orderDto.Note;
            order.TotalAmount = orderDto.Items.Sum(i => i.LineTotal);

            foreach (var itemDto in orderDto.Items)
            {
                var product = await context.Products.FindAsync(itemDto.ProductId);
                product!.Quantity -= itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = itemDto.ProductId,
                    UnitPrice = itemDto.UnitPrice,
                    Quantity = itemDto.Quantity,
                    LineTotal = itemDto.LineTotal
                };

                context.OrderItems.Add(orderItem);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        _authorizationService.EnsureCanDelete("Orders");

        using var context = new ShopAccessoriesContext();
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                throw new InvalidOperationException("Order not found");

            foreach (var item in order.OrderItems)
            {
                var product = await context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                }
            }

            context.OrderItems.RemoveRange(order.OrderItems);
            context.Orders.Remove(order);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name,
            Note = order.Note,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            CreatedByUsername = order.CreatedByUser.Username,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                ProductId = oi.ProductId,
                ProductName = oi.Product.Name,
                ProductCode = oi.Product.Code,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity,
                LineTotal = oi.LineTotal
            }).ToList()
        };
    }
}
