using System.Windows.Controls;

namespace BL5_PRN212_MustPass_Project.Views;

public partial class UsersView : UserControl
{
    public UsersView()
    {
        InitializeComponent();
    }

    private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ViewModels.UsersViewModel viewModel)
        {
            viewModel.RoleChangedCommand.Execute(null);
        }
    }
}
