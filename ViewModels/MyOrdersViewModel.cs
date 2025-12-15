using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class MyOrdersViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<OrderDto> _orders = new();

    [ObservableProperty]
    private OrderDto? _selectedOrder;

    public MyOrdersViewModel(IOrderService orderService, IAuthService authService)
    {
        _orderService = orderService;
        _authService = authService;
        _ = LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        try
        {
            var customerId = _authService.CurrentUser?.CustomerId;
            if (customerId == null)
            {
                MessageBox.Show("Customer information not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var orders = await _orderService.GetOrdersByCustomerAsync(customerId.Value);
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedOrder == null) return;

        var details = $"Order #{SelectedOrder.OrderId}\n\n";
        details += $"Created: {SelectedOrder.CreatedAt:g}\n";
        details += $"Created By: {SelectedOrder.CreatedByUsername}\n";
        details += $"Note: {SelectedOrder.Note}\n\n";
        details += "Items:\n";

        foreach (var item in SelectedOrder.Items)
        {
            details += $"- {item.ProductName} ({item.ProductCode}): {item.Quantity} x {item.UnitPrice:C} = {item.LineTotal:C}\n";
        }

        details += $"\nTotal: {SelectedOrder.TotalAmount:C}";

        MessageBox.Show(details, "Order Details", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
