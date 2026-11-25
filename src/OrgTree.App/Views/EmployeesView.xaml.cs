using System.Windows.Controls;
using OrgTree.App.Services;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

internal sealed partial class EmployeesView : UserControl
{
    public EmployeesView()
    {
        InitializeComponent();
        var viewModel = new EmployeesViewModel(new DialogService());
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.Load();
    }
}
