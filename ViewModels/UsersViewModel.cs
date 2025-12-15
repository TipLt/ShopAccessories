using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Models;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class UsersViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly ICustomerService _customerService;
    private readonly IAuthorizationService _authorizationService;

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private User? _selectedUser;

    [ObservableProperty]
    private string[] _roles = new[] { "Admin", "Staff", "Customer" };

    [ObservableProperty]
    private bool _canModify;

    [ObservableProperty]
    private bool _isCustomerRole;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _customerPhone = string.Empty;

    [ObservableProperty]
    private string _customerEmail = string.Empty;

    public UsersViewModel(
        IUserService userService, 
        ICustomerService customerService,
        IAuthorizationService authorizationService)
    {
        _userService = userService;
        _customerService = customerService;
        _authorizationService = authorizationService;

        CanModify = _authorizationService.CanCreate("Users");
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await LoadUsersAsync();
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Add()
    {
        SelectedUser = new User 
        { 
            Username = string.Empty, 
            Password = string.Empty,
            FullName = string.Empty,
            Role = "Staff",
            IsActive = true
        };
        ClearCustomerInfo();
        IsCustomerRole = false;
    }

    partial void OnSelectedUserChanged(User? value)
    {
        if (value != null)
        {
            IsCustomerRole = value.Role == "Customer";
            
            if (value.UserId > 0 && value.CustomerId != null)
            {
                // Load customer data - prefer navigation property if available
                if (value.Customer != null)
                {
                    CustomerName = value.Customer.Name;
                    CustomerPhone = value.Customer.Phone ?? string.Empty;
                    CustomerEmail = value.Customer.Email ?? string.Empty;
                }
                else
                {
                    // If navigation property not loaded, load from service
                    _ = LoadCustomerForEditAsync(value.CustomerId.Value);
                }
            }
            else
            {
                ClearCustomerInfo();
            }
        }
        else
        {
            IsCustomerRole = false;
            ClearCustomerInfo();
        }
    }

    private async Task LoadCustomerForEditAsync(int customerId)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(customerId);
            if (customer != null)
            {
                CustomerName = customer.Name;
                CustomerPhone = customer.Phone ?? string.Empty;
                CustomerEmail = customer.Email ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RoleChanged()
    {
        if (SelectedUser != null)
        {
            IsCustomerRole = SelectedUser.Role == "Customer";
            
            if (!IsCustomerRole)
            {
                SelectedUser.CustomerId = null;
                ClearCustomerInfo();
            }
        }
    }

    private void ClearCustomerInfo()
    {
        CustomerName = string.Empty;
        CustomerPhone = string.Empty;
        CustomerEmail = string.Empty;
    }

    [RelayCommand]
    private async Task Save()
    {
        // Capture the current selected user to avoid issues if selection changes during async operations
        var userToSave = SelectedUser;
        if (userToSave == null) return;

        try
        {
            if (string.IsNullOrWhiteSpace(userToSave.Username))
            {
                MessageBox.Show("Username is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(userToSave.Password))
            {
                MessageBox.Show("Password is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(userToSave.Role))
            {
                MessageBox.Show("Role is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (userToSave.Role == "Customer")
            {
                if (string.IsNullOrWhiteSpace(CustomerName))
                {
                    MessageBox.Show("Customer name is required for Customer role", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new customer if user is new or doesn't have linked customer
                if (userToSave.UserId == 0 || userToSave.CustomerId == null)
                {
                    var newCustomer = new Customer
                    {
                        Name = CustomerName.Trim(),
                        Phone = string.IsNullOrWhiteSpace(CustomerPhone) ? null : CustomerPhone.Trim(),
                        Email = string.IsNullOrWhiteSpace(CustomerEmail) ? null : CustomerEmail.Trim(),
                        IsActive = true
                    };

                    var createdCustomer = await _customerService.CreateAsync(newCustomer);
                    userToSave.CustomerId = createdCustomer.CustomerId;
                    
                    // Sync User.FullName with Customer.Name
                    userToSave.FullName = CustomerName.Trim();
                }
                else
                {
                    // Update existing customer
                    var existingCustomer = await _customerService.GetByIdAsync(userToSave.CustomerId.Value);
                    if (existingCustomer != null)
                    {
                        existingCustomer.Name = CustomerName.Trim();
                        existingCustomer.Phone = string.IsNullOrWhiteSpace(CustomerPhone) ? null : CustomerPhone.Trim();
                        existingCustomer.Email = string.IsNullOrWhiteSpace(CustomerEmail) ? null : CustomerEmail.Trim();
                        await _customerService.UpdateAsync(existingCustomer);
                        
                        // Sync User.FullName with Customer.Name
                        userToSave.FullName = CustomerName.Trim();
                    }
                }
            }
            else
            {
                // For Admin/Staff roles, clear customer link
                userToSave.CustomerId = null;
            }

            // Save the user ID before clearing selection
            var savedUserId = userToSave.UserId;
            var isNewUser = savedUserId == 0;

            if (isNewUser)
            {
                await _userService.CreateAsync(userToSave);
                MessageBox.Show("User created successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _userService.UpdateAsync(userToSave);
                MessageBox.Show("User updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Clear selection to deselect current item
            SelectedUser = null;
            ClearCustomerInfo();
            
            // Reload all users with fresh data from database
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (SelectedUser == null || SelectedUser.UserId == 0) return;

        var result = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _userService.DeleteAsync(SelectedUser.UserId);
            MessageBox.Show("User deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            SelectedUser = null;
            ClearCustomerInfo();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedUser = null;
        ClearCustomerInfo();
    }
}
