namespace BL5_PRN212_MustPass_Project.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync();
    Task<List<OrderDto>> GetOrdersByCustomerAsync(int customerId);
    Task<OrderDto?> GetByIdAsync(int id);
    Task<OrderDto> CreateAsync(OrderDto orderDto);
    Task UpdateAsync(OrderDto orderDto);
    Task DeleteAsync(int id);
}
