using System.Windows;
using OrgTree.App.ViewModels;
using OrgTree.App.Views;

namespace OrgTree.App.Services;

internal sealed class DialogService : IDialogService
{
    public bool Confirm(string title, string message)
    {
        var result = MessageBox.Show(
            Application.Current.MainWindow,
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        return result == MessageBoxResult.Yes;
    }

    public bool ShowEditor(DepartmentEditViewModel viewModel)
    {
        return ShowDialog(new DepartmentEditWindow(viewModel));
    }

    public bool ShowEditor(EmployeeEditViewModel viewModel)
    {
        return ShowDialog(new EmployeeEditWindow(viewModel));
    }

    private static bool ShowDialog(Window window)
    {
        window.Owner = Application.Current.MainWindow;
        return window.ShowDialog() == true;
    }
}
