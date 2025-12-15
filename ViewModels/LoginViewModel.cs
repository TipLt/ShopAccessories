using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BL5_PRN212_MustPass_Project.Services;

namespace BL5_PRN212_MustPass_Project.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task Login(object? parameter)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter username";
            return;
        }

        var passwordBox = parameter as System.Windows.Controls.PasswordBox;
        var password = passwordBox?.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Please enter password";
            return;
        }

        try
        {
            var user = await _authService.LoginAsync(Username, password);

            if (user == null)
            {
                ErrorMessage = "Invalid username or password, or account is inactive";
                return;
            }

            var mainWindow = _serviceProvider.GetService(typeof(Views.MainWindow)) as Window;
            mainWindow?.Show();

            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)?.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
    }
}
