using System.Windows;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

public partial class EmployeeEditWindow : Window
{
    public EmployeeEditWindow(EmployeeEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += result => DialogResult = result;
    }
}
