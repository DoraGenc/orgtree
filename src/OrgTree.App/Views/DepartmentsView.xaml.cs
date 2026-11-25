using System.Windows.Controls;
using OrgTree.App.Services;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

internal sealed partial class DepartmentsView : UserControl
{
    public DepartmentsView()
    {
        InitializeComponent();
        var viewModel = new DepartmentsViewModel(new DialogService());
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.Load();
    }
}
