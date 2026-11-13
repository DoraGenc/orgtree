using CommunityToolkit.Mvvm.ComponentModel;

namespace OrgTree.App.ViewModels;

public partial class OrgChartNode : ObservableObject
{
    public required string Name { get; init; }
    public string? LeaderName { get; init; }
    public required string BoxColor { get; init; }
    public bool HasParent { get; init; }
    public bool ShowLeftLine { get; set; }
    public bool ShowRightLine { get; set; }
    public List<OrgChartNode> Children { get; } = new();
    public bool HasChildren => Children.Count > 0;

    [ObservableProperty]
    private bool _isExpanded = true;
}
