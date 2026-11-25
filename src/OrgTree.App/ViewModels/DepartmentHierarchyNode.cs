namespace OrgTree.App.ViewModels;

internal sealed class DepartmentHierarchyNode : HierarchyNode
{
    public int? Id { get; init; }

    public required string Name { get; init; }

    public string? ColorHex { get; init; }

    public IReadOnlyList<HierarchyNode> Children { get; init; } = [];

    public string BoxColor => ColorHex ?? DisplayTexts.DefaultDepartmentColor;

    public bool IsDepartment => Id != null;

    public override string Key => $"D{Id}";
}
