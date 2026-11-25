namespace OrgTree.App.ViewModels;

internal sealed class EmployeeRow
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string? PhoneNumber { get; init; }

    public string? DepartmentName { get; init; }

    public string? DepartmentHead { get; init; }
}
