using System.Windows;
using OrgTree.App.ViewModels;

namespace OrgTree.App;

internal sealed partial class MainWindow : Window
{
    private const double InitialScreenShare = 0.8;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();

        var workArea = SystemParameters.WorkArea;
        Width = workArea.Width * InitialScreenShare;
        Height = workArea.Height * InitialScreenShare;
    }
}
