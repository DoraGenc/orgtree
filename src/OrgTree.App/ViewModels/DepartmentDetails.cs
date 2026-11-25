namespace OrgTree.App.ViewModels;

internal sealed class DepartmentDetails
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string? ColorHex { get; init; }

    public string? LeaderName { get; init; }

    public string? ParentDepartmentName { get; init; }
}
