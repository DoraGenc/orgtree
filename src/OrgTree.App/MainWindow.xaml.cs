using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrgTree.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var workArea = SystemParameters.WorkArea;

        Width = workArea.Width * 0.8;
        Height = workArea.Height * 0.8;
    }
}