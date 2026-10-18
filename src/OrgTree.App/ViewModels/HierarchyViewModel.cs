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

        var departments = db.Departments.OrderBy(d => d.Name).ToList();
        var employees = db.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();
        var departmentIds = departments.Select(d => d.Id).ToHashSet();
        var visited = new HashSet<int>();

        foreach (var root in departments.Where(d => d.ParentDepartmentId == null || !departmentIds.Contains(d.ParentDepartmentId.Value)))
        {
            visited.Add(root.Id);
            Roots.Add(BuildDepartment(root, departments, employees, visited, keyToRestore));
        }

        var unassigned = employees
            .Where(e => e.DepartmentId == null || !departmentIds.Contains(e.DepartmentId.Value))
            .ToList();

        if (unassigned.Count > 0)
        {
            var node = new DepartmentHierarchyNode { Name = "Ohne Abteilung" };
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
        if (selected is EmployeeHierarchyNode employeeNode)
        {
            var viewModel = new EmployeeEditViewModel(employeeNode.Id);
            viewModel.CloseRequested += OnSaved;
            Detail = viewModel;
            _selectedKey = employeeNode.Key;
        }
        else if (selected is DepartmentHierarchyNode departmentNode && departmentNode.Id != null)
        {
            var viewModel = new DepartmentEditViewModel(departmentNode.Id);
            viewModel.CloseRequested += OnSaved;
            Detail = viewModel;
            _selectedKey = departmentNode.Key;
        }
        else
        {
            Detail = null;
            _selectedKey = null;
        }
    }

    private void OnSaved(bool saved)
    {
        if (saved)
        {
            Load();
        }
    }
}
