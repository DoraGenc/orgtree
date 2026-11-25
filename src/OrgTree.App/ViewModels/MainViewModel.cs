using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OrgTree.App.ViewModels;

internal sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex = (int)AppPage.Start;

    [RelayCommand]
    private void Navigate(AppPage page)
    {
        SelectedTabIndex = (int)page;
    }
}
