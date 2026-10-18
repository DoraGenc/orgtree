namespace OrgTree.Core;

public class Employee
{
    public int Id { get; set; }
    required public string FirstName { get; set; }
    required public string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public int? DepartmentId { get; set; }
    public bool? DepartmentHead { get; set; }
}