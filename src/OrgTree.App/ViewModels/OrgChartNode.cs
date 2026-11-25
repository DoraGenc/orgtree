using CommunityToolkit.Mvvm.ComponentModel;

namespace OrgTree.App.ViewModels;

internal sealed partial class OrgChartNode : ObservableObject
{
    [ObservableProperty]
    private bool _isExpanded = true;

    public required string Name { get; init; }

    public string? LeaderName { get; init; }

    public required string BoxColor { get; init; }

    public bool HasParent { get; init; }

    public bool ShowLeftLine { get; init; }

    public bool ShowRightLine { get; init; }

    public IReadOnlyList<OrgChartNode> Children { get; init; } = [];

    public bool HasChildren => Children.Count > 0;
}
