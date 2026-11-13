using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class HierarchyViewModel : ObservableObject
{
    private string? _selectedKey;

    public ObservableCollection<DepartmentHierarchyNode> Roots { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSelectionHint))]
    private object? _detail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSelectionHint))]
    private bool _isEmpty;

    public bool ShowSelectionHint => Detail == null && !IsEmpty;

    public void Load()
    {
        var keyToRestore = _selectedKey;

        Roots.Clear();
        Detail = null;

        using var db = new OrgTreeContext();

        var departments = db.Departments.OrderBy(department => department.Name).ToList();
        var employees = db.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();
        var departmentIds = departments.Select(d => d.Id).ToHashSet();
        var visited = new HashSet<int>();

        foreach (var root in departments.Where(d => d.ParentDepartmentId == null || !departmentIds.Contains(d.ParentDepartmentId.Value)))
        {
            visited.Add(root.Id);
            Roots.Add(BuildDepartment(root, departments, employees, visited, keyToRestore));
        }

        var unassigned = employees
            .Where(employee => employee.DepartmentId == null
                || !departmentIds.Contains(employee.DepartmentId.Value))
            .ToList();

        if (unassigned.Count > 0)
        {
            var node = new DepartmentHierarchyNode { Name = "Keine Abteilung zugeordnet" };
            foreach (var employee in unassigned)
            {
                node.Children.Add(BuildEmployee(employee, isLeader: false, keyToRestore));
            }

            Roots.Add(node);
        }

        IsEmpty = Roots.Count == 0;
    }

    private static DepartmentHierarchyNode BuildDepartment(
        Department department,
        List<Department> departments,
        List<Employee> employees,
        HashSet<int> visited,
        string? keyToRestore)
    {
        var node = new DepartmentHierarchyNode
        {
            Id = department.Id,
            Name = department.Name,
            ColorHex = department.ColorHex
        };
        node.IsSelected = node.Key == keyToRestore;

        var members = employees
            .Where(e => e.DepartmentId == department.Id)
            .OrderByDescending(e => e.Id == department.LeaderId);

        foreach (var employee in members)
        {
            node.Children.Add(BuildEmployee(employee, employee.Id == department.LeaderId, keyToRestore));
        }

        foreach (var child in departments.Where(d => d.ParentDepartmentId == department.Id))
        {
            if (visited.Add(child.Id))
            {
                node.Children.Add(BuildDepartment(child, departments, employees, visited, keyToRestore));
            }
        }

        return node;
    }

    private static EmployeeHierarchyNode BuildEmployee(Employee employee, bool isLeader, string? keyToRestore)
    {
        var node = new EmployeeHierarchyNode
        {
            Id = employee.Id,
            Name = $"{employee.FirstName} {employee.LastName}",
            IsLeader = isLeader
        };
        node.IsSelected = node.Key == keyToRestore;
        return node;
    }

    public void Select(object? selected)
    {
        using var db = new OrgTreeContext();

        if (selected is EmployeeHierarchyNode employeeNode
            && db.Employees.Find(employeeNode.Id) is Employee employee)
        {
            Detail = BuildEmployeeDetails(db, employee);
            _selectedKey = employeeNode.Key;
        }
        else if (selected is DepartmentHierarchyNode departmentNode
            && departmentNode.Id != null
            && db.Departments.Find(departmentNode.Id) is Department department)
        {
            Detail = BuildDepartmentDetails(db, department);
            _selectedKey = departmentNode.Key;
        }
        else
        {
            Detail = null;
            _selectedKey = null;
        }
    }

    private static EmployeeDetails BuildEmployeeDetails(OrgTreeContext db, Employee employee)
    {
        var department = employee.DepartmentId == null
            ? null
            : db.Departments.Find(employee.DepartmentId);

        return new EmployeeDetails
        {
            Id = employee.Id,
            DisplayName = $"{employee.LastName}, {employee.FirstName}",
            LastName = employee.LastName,
            FirstName = employee.FirstName,
            PhoneNumber = employee.PhoneNumber,
            DepartmentName = department?.Name,
            DepartmentHead = department?.LeaderId == employee.Id ? "Ja" : "Nein"
        };
    }

    private static DepartmentDetails BuildDepartmentDetails(OrgTreeContext db, Department department)
    {
        var leader = department.LeaderId == null
            ? null
            : db.Employees.Find(department.LeaderId);

        var parent = department.ParentDepartmentId == null
            ? null
            : db.Departments.Find(department.ParentDepartmentId);

        return new DepartmentDetails
        {
            Id = department.Id,
            Name = department.Name,
            ColorHex = department.ColorHex,
            LeaderName = leader == null ? null : $"{leader.LastName}, {leader.FirstName}",
            ParentDepartmentName = parent?.Name
        };
    }
}
