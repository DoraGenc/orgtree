namespace OrgTree.App.ViewModels;

internal sealed class DepartmentRow
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string? LeaderName { get; init; }

    public string? ParentDepartmentName { get; init; }

    public string? ColorHex { get; init; }
}
