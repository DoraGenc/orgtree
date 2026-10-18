using System.Windows;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

public partial class DepartmentEditWindow : Window
{
    public DepartmentEditWindow(DepartmentEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += result => DialogResult = result;
    }
}
