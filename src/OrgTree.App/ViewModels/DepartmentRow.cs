namespace OrgTree.App.ViewModels;

public class DepartmentRow
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? LeaderName { get; init; }
    public string? ParentDepartmentName { get; init; }
    public string? ColorHex { get; init; }
}
