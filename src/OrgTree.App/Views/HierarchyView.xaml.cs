using System.Windows;
using System.Windows.Controls;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Views;

public partial class HierarchyView : UserControl
{
    private readonly HierarchyViewModel _viewModel = new();

    public HierarchyView()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += (_, _) => _viewModel.Load();
    }

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _viewModel.Select(e.NewValue);
    }
}
