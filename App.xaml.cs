using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BL5_PRN212_MustPass_Project.Services;
using BL5_PRN212_MustPass_Project.ViewModels;
using BL5_PRN212_MustPass_Project.Views;

namespace BL5_PRN212_MustPass_Project
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IAuthService, AuthService>();
                    services.AddSingleton<IAuthorizationService, AuthorizationService>();

                    services.AddTransient<ICategoryService, CategoryService>();
                    services.AddTransient<IProductService, ProductService>();
                    services.AddTransient<ICustomerService, CustomerService>();
                    services.AddTransient<IOrderService, OrderService>();
                    services.AddTransient<IUserService, UserService>();

                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<CategoriesViewModel>();
                    services.AddTransient<ProductsViewModel>();
                    services.AddTransient<CustomersViewModel>();
                    services.AddTransient<OrdersViewModel>();
                    services.AddTransient<OrderEditViewModel>();
                    services.AddTransient<MyOrdersViewModel>();
                    services.AddTransient<MyProfileViewModel>();
                    services.AddTransient<UsersViewModel>();

                    services.AddTransient<LoginWindow>(provider => 
                    {
                        var window = new LoginWindow();
                        window.DataContext = provider.GetRequiredService<LoginViewModel>();
                        return window;
                    });

                    services.AddTransient<Views.MainWindow>(provider =>
                    {
                        var window = new Views.MainWindow();
                        window.DataContext = provider.GetRequiredService<MainViewModel>();
                        return window;
                    });

                    services.AddTransient<CategoriesView>(provider =>
                    {
                        var view = new CategoriesView();
                        view.DataContext = provider.GetRequiredService<CategoriesViewModel>();
                        return view;
                    });

                    services.AddTransient<ProductsView>(provider =>
                    {
                        var view = new ProductsView();
                        view.DataContext = provider.GetRequiredService<ProductsViewModel>();
                        return view;
                    });

                    services.AddTransient<CustomersView>(provider =>
                    {
                        var view = new CustomersView();
                        view.DataContext = provider.GetRequiredService<CustomersViewModel>();
                        return view;
                    });

                    services.AddTransient<OrdersView>(provider =>
                    {
                        var view = new OrdersView();
                        view.DataContext = provider.GetRequiredService<OrdersViewModel>();
                        return view;
                    });

                    services.AddTransient<OrderEditDialog>(provider =>
                    {
                        var dialog = new OrderEditDialog();
                        dialog.DataContext = provider.GetRequiredService<OrderEditViewModel>();
                        return dialog;
                    });

                    services.AddTransient<MyOrdersView>(provider =>
                    {
                        var view = new MyOrdersView();
                        view.DataContext = provider.GetRequiredService<MyOrdersViewModel>();
                        return view;
                    });

                    services.AddTransient<MyProfileView>(provider =>
                    {
                        var view = new MyProfileView();
                        view.DataContext = provider.GetRequiredService<MyProfileViewModel>();
                        return view;
                    });

                    services.AddTransient<UsersView>(provider =>
                    {
                        var view = new UsersView();
                        view.DataContext = provider.GetRequiredService<UsersViewModel>();
                        return view;
                    });
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var loginViewModel = _host.Services.GetRequiredService<LoginViewModel>();
            var loginWindow = new LoginWindow
            {
                DataContext = loginViewModel
            };

            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }

}
