namespace OrgTree.App.ViewModels;

public class EmployeeHierarchyNode : HierarchyNode
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public bool IsLeader { get; init; }
    public override string Key => $"E{Id}";
}
