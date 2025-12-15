using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Models;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class OrderEditViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty]
    private OrderDto _orderDto = new() { Items = new List<OrderItemDto>() };

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<OrderItemDto> _orderItems = new();

    [ObservableProperty]
    private OrderItemDto? _selectedItem;

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private int _newItemQuantity = 1;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private bool _isEditMode;

    public OrderEditViewModel(
        IOrderService orderService,
        ICustomerService customerService,
        IProductService productService,
        IAuthorizationService authorizationService)
    {
        _orderService = orderService;
        _customerService = customerService;
        _productService = productService;
        _authorizationService = authorizationService;

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }

            var products = await _productService.GetAllAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void LoadOrderForCreate()
    {
        OrderDto = new OrderDto { Items = new List<OrderItemDto>() };
        OrderItems.Clear();
        IsEditMode = false;
        TotalAmount = 0;
    }

    public void LoadOrderForEdit(OrderDto order)
    {
        OrderDto = order;
        OrderItems.Clear();
        if (order?.Items != null)
        {
            foreach (var item in order.Items)
            {
                OrderItems.Add(item);
            }
        }
        IsEditMode = true;
        CalculateTotal();
    }

    partial void OnOrderDtoChanged(OrderDto value)
    {
        OrderItems.Clear();
        if (value?.Items != null)
        {
            foreach (var item in value.Items)
            {
                OrderItems.Add(item);
            }
        }
        CalculateTotal();
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Please select a product", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NewItemQuantity <= 0)
        {
            MessageBox.Show("Quantity must be greater than 0", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var existingItem = OrderItems.FirstOrDefault(i => i.ProductId == SelectedProduct.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += NewItemQuantity;
            existingItem.LineTotal = existingItem.Quantity * existingItem.UnitPrice;
        }
        else
        {
            var item = new OrderItemDto
            {
                ProductId = SelectedProduct.ProductId,
                ProductName = SelectedProduct.Name,
                ProductCode = SelectedProduct.Code,
                UnitPrice = SelectedProduct.Price,
                Quantity = NewItemQuantity,
                LineTotal = SelectedProduct.Price * NewItemQuantity
            };

            OrderItems.Add(item);
        }

        SelectedProduct = null;
        NewItemQuantity = 1;
        CalculateTotal();
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedItem == null) return;

        OrderItems.Remove(SelectedItem);
        SelectedItem = null;
        CalculateTotal();
    }

    private void CalculateTotal()
    {
        TotalAmount = OrderItems.Sum(i => i.LineTotal);
    }

    [RelayCommand]
    private async Task SaveOrder()
    {
        await SaveAsync();
    }

    public async Task<bool> SaveAsync()
    {
        try
        {
            if (OrderItems.Count == 0)
            {
                MessageBox.Show("Order must have at least one item", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            OrderDto.Items = OrderItems.ToList();
            OrderDto.TotalAmount = TotalAmount;

            if (IsEditMode)
            {
                await _orderService.UpdateAsync(OrderDto);
                MessageBox.Show("Order updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _orderService.CreateAsync(OrderDto);
                MessageBox.Show("Order created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    partial void OnSelectedItemChanged(OrderItemDto? value)
    {
        if (value != null)
        {
            CalculateTotal();
        }
    }
}
