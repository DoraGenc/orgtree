using CommunityToolkit.Mvvm.ComponentModel;

namespace OrgTree.App.ViewModels;

public abstract partial class HierarchyNode : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isExpanded = true;

    public abstract string Key { get; }
}
