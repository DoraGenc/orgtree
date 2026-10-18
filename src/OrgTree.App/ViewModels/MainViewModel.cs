using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OrgTree.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex;

    [RelayCommand]
    private void Navigate(AppPage page)
    {
        SelectedTabIndex = (int)page;
    }
}
