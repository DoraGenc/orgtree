using System.Windows.Controls;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

public partial class DepartmentsView : UserControl
{
    public DepartmentsView()
    {
        InitializeComponent();
        var viewModel = new DepartmentsViewModel();
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.Load();
    }
}
