using System.Windows;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

internal sealed partial class EmployeeEditWindow : Window
{
    public EmployeeEditWindow(EmployeeEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += (_, saved) => DialogResult = saved;
    }
}
