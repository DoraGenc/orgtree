using System.Collections.ObjectModel;

namespace OrgTree.App.ViewModels;

public class DepartmentHierarchyNode : HierarchyNode
{
    public int? Id { get; init; }
    public required string Name { get; init; }
    public string? ColorHex { get; init; }
    public string BoxColor => ColorHex ?? DepartmentColors.Default;
    public bool IsDepartment => Id != null;
    public ObservableCollection<HierarchyNode> Children { get; } = new();
    public override string Key => $"D{Id}";
}
