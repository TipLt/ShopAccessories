using System.Windows;
using BL5_PRN212_MustPass_Project.ViewModels;

namespace BL5_PRN212_MustPass_Project.Views;

public partial class OrderEditDialog : Window
{
    public OrderEditDialog()
    {
        InitializeComponent();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is OrderEditViewModel viewModel)
        {
            var success = await viewModel.SaveAsync();
            if (success)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
