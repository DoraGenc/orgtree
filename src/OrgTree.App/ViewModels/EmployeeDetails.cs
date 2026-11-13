namespace OrgTree.App.ViewModels;

public class EmployeeDetails
{
    public int Id { get; init; }
    public required string DisplayName { get; init; }
    public required string LastName { get; init; }
    public required string FirstName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? DepartmentName { get; init; }
    public required string DepartmentHead { get; init; }
}
