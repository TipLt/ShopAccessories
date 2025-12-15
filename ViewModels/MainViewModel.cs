using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Services;
using BL5_PRN212_MustPass_Project.Views;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private bool _canAccessUsers;

    [ObservableProperty]
    private bool _canAccessCategories;

    [ObservableProperty]
    private bool _canAccessProducts;

    [ObservableProperty]
    private bool _canAccessCustomers;

    [ObservableProperty]
    private bool _canAccessOrders;

    [ObservableProperty]
    private bool _canAccessMyOrders;

    [ObservableProperty]
    private bool _canAccessMyProfile;

    public MainViewModel(
        IAuthService authService, 
        IAuthorizationService authorizationService,
        IServiceProvider serviceProvider)
    {
        _authService = authService;
        _authorizationService = authorizationService;
        _serviceProvider = serviceProvider;

        InitializePermissions();
    }

    private void InitializePermissions()
    {
        var user = _authService.CurrentUser;
        if (user == null) return;

        // For Customer role, show Customer name instead of User.FullName
        var displayName = user.FullName ?? user.Username;
        if (user.Role == "Customer" && user.Customer != null)
        {
            displayName = user.Customer.Name;
        }

        WelcomeMessage = $"Welcome, {displayName} ({user.Role})";

        if (_authorizationService.IsAdmin())
        {
            CanAccessUsers = true;
            CanAccessCategories = true;
            CanAccessProducts = true;
            CanAccessCustomers = true;
            CanAccessOrders = true;
        }
        else if (_authorizationService.IsStaff())
        {
            CanAccessCategories = true;
            CanAccessProducts = true;
            CanAccessCustomers = true;
            CanAccessOrders = true;
        }
        else if (_authorizationService.IsCustomer())
        {
            CanAccessCategories = true;
            CanAccessProducts = true;
            CanAccessMyOrders = true;
            CanAccessMyProfile = true;
        }
    }

    [RelayCommand]
    private void ShowUsers()
    {
        var view = _serviceProvider.GetService(typeof(UsersView));
        CurrentView = view;
    }

    [RelayCommand]
    private void ShowCategories()
    {
        var view = _serviceProvider.GetService(typeof(CategoriesView));
        CurrentView = view;
    }

    [RelayCommand]
    private void ShowProducts()
    {
        var view = _serviceProvider.GetService(typeof(ProductsView));
        CurrentView = view;
    }

    [RelayCommand]
    private void ShowCustomers()
    {
        var view = _serviceProvider.GetService(typeof(CustomersView));
        CurrentView = view;
    }

    [RelayCommand]
    private void ShowOrders()
    {
        var view = _serviceProvider.GetService(typeof(OrdersView));
        CurrentView = view;
    }

    [RelayCommand]
    private void ShowMyOrders()
    {
        var view = _serviceProvider.GetService(typeof(MyOrdersView)) as MyOrdersView;
        if (view?.DataContext is MyOrdersViewModel viewModel)
        {
            _ = viewModel.LoadOrdersCommand.ExecuteAsync(null);
        }
        CurrentView = view;
    }

    [RelayCommand]
    private void ShowMyProfile()
    {
        var view = _serviceProvider.GetService(typeof(MyProfileView)) as MyProfileView;
        if (view?.DataContext is MyProfileViewModel viewModel)
        {
            _ = viewModel.LoadProfileCommand.ExecuteAsync(null);
        }
        CurrentView = view;
    }

    [RelayCommand]
    private void Logout()
    {
        _authService.Logout();

        var loginWindow = _serviceProvider.GetService(typeof(LoginWindow)) as Window;
        loginWindow?.Show();

        Application.Current.Windows.OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?.Close();
    }
}
