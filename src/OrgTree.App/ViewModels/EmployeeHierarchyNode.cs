namespace OrgTree.App.ViewModels;

internal sealed class EmployeeHierarchyNode : HierarchyNode
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public bool IsLeader { get; init; }

    public override string Key => $"E{Id}";
}
