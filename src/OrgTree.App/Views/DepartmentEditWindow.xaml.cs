using System.Windows;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

internal sealed partial class DepartmentEditWindow : Window
{
    public DepartmentEditWindow(DepartmentEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += (_, saved) => DialogResult = saved;
    }
}
