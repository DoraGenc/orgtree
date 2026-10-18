using System.Windows.Controls;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

public partial class EmployeesView : UserControl
{
    public EmployeesView()
    {
        InitializeComponent();
        var viewModel = new EmployeesViewModel();
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.Load();
    }
}
