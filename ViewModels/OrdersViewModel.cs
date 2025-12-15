using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Services;
using BL5_PRN212_MustPass_Project.Views;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<OrderDto> _orders = new();

    [ObservableProperty]
    private OrderDto? _selectedOrder;

    [ObservableProperty]
    private bool _canModify;

    [ObservableProperty]
    private bool _canCreate;

    public OrdersViewModel(
        IOrderService orderService, 
        IAuthorizationService authorizationService,
        IServiceProvider serviceProvider)
    {
        _orderService = orderService;
        _authorizationService = authorizationService;
        _serviceProvider = serviceProvider;

        CanModify = _authorizationService.CanUpdate("Orders");
        CanCreate = _authorizationService.CanCreate("Orders");
        _ = LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        try
        {
            var orders = await _orderService.GetAllAsync();
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
    private void Add()
    {
        var dialog = _serviceProvider.GetService(typeof(OrderEditDialog)) as OrderEditDialog;
        if (dialog != null)
        {
            var viewModel = dialog.DataContext as OrderEditViewModel;
            if (viewModel != null)
            {
                viewModel.LoadOrderForCreate();
            }

            if (dialog.ShowDialog() == true)
            {
                _ = LoadOrdersAsync();
            }
        }
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedOrder == null) return;

        var dialog = _serviceProvider.GetService(typeof(OrderEditDialog)) as OrderEditDialog;
        if (dialog != null)
        {
            var viewModel = dialog.DataContext as OrderEditViewModel;
            if (viewModel != null)
            {
                viewModel.LoadOrderForEdit(SelectedOrder);
            }

            if (dialog.ShowDialog() == true)
            {
                _ = LoadOrdersAsync();
            }
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedOrder == null) return;

        var result = MessageBox.Show("Are you sure you want to delete this order?", "Confirm Delete", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _orderService.DeleteAsync(SelectedOrder.OrderId);
            MessageBox.Show("Order deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedOrder == null) return;

        var details = $"Order #{SelectedOrder.OrderId}\n\n";
        details += $"Customer: {SelectedOrder.CustomerName ?? "Walk-in"}\n";
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
