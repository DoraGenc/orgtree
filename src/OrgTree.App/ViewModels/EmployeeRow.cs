namespace OrgTree.App.ViewModels;

public class EmployeeRow
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? PhoneNumber { get; init; }
    public string? DepartmentName { get; init; }
    public string? DepartmentHead { get; init; }
}
