using System.Windows.Controls;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

public partial class OrgChartView : UserControl
{
    public OrgChartView()
    {
        InitializeComponent();
        var viewModel = new OrgChartViewModel();
        DataContext = viewModel;
        Loaded += (_, _) => viewModel.Load();
    }
}
