using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

internal sealed partial class HierarchyViewModel : ObservableObject
{
    private string? _selectedKey;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSelectionHint))]
    private object? _detail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSelectionHint))]
    private bool _isEmpty;

    public ObservableCollection<DepartmentHierarchyNode> Roots { get; } = [];

    public bool ShowSelectionHint => Detail == null && !IsEmpty;

    public void Load()
    {
        Roots.Clear();
        Detail = null;

        using var db = new OrgTreeContext();

        var departments = db.Departments.OrderBy(department => department.Name).ToList();
        var employees = db.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();

        var builder = new TreeBuilder(departments, employees, _selectedKey);
        foreach (var root in builder.BuildRoots())
        {
            Roots.Add(root);
        }

        IsEmpty = Roots.Count == 0;
    }

    public void Select(object? selected)
    {
        using var db = new OrgTreeContext();

        switch (selected)
        {
            case EmployeeHierarchyNode node when db.Employees.Find(node.Id) is Employee employee:
                Detail = BuildEmployeeDetails(db, employee);
                _selectedKey = node.Key;
                break;

            case DepartmentHierarchyNode { Id: int departmentId } node when db.Departments.Find(departmentId) is Department department:
                Detail = BuildDepartmentDetails(db, department);
                _selectedKey = node.Key;
                break;

            default:
                Detail = null;
                _selectedKey = null;
                break;
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
            DisplayName = employee.DisplayName,
            LastName = employee.LastName,
            FirstName = employee.FirstName,
            PhoneNumber = employee.PhoneNumber,
            DepartmentName = department?.Name,
            DepartmentHead = DisplayTexts.YesNo(department?.LeaderId == employee.Id)
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
            LeaderName = leader?.DisplayName,
            ParentDepartmentName = parent?.Name
        };
    }

    private sealed class TreeBuilder
    {
        private readonly List<Department> _departments;
        private readonly HashSet<int> _departmentIds;
        private readonly ILookup<int?, Department> _departmentsByParent;
        private readonly List<Employee> _employees;
        private readonly HashSet<int> _visited = [];
        private readonly string? _selectedKey;

        public TreeBuilder(List<Department> departments, List<Employee> employees, string? selectedKey)
        {
            _departments = departments;
            _departmentIds = departments.Select(department => department.Id).ToHashSet();
            _departmentsByParent = departments.ToLookup(department => department.ParentDepartmentId);
            _employees = employees;
            _selectedKey = selectedKey;
        }

        public IEnumerable<DepartmentHierarchyNode> BuildRoots()
        {
            var roots = _departments.Where(department => !HasExistingParent(department)).ToList();
            foreach (var root in roots)
            {
                if (_visited.Add(root.Id))
                {
                    yield return BuildDepartment(root);
                }
            }

            var unassigned = _employees.Where(employee => !BelongsToExistingDepartment(employee)).ToList();
            if (unassigned.Count > 0)
            {
                yield return new DepartmentHierarchyNode
                {
                    Name = DisplayTexts.NoDepartment,
                    Children = unassigned.Select(employee => BuildEmployee(employee, isLeader: false)).ToList()
                };
            }
        }

        private DepartmentHierarchyNode BuildDepartment(Department department)
        {
            var children = new List<HierarchyNode>();

            var members = _employees
                .Where(employee => employee.DepartmentId == department.Id)
                .OrderByDescending(employee => employee.Id == department.LeaderId);

            foreach (var member in members)
            {
                children.Add(BuildEmployee(member, isLeader: member.Id == department.LeaderId));
            }

            foreach (var child in _departmentsByParent[department.Id])
            {
                if (_visited.Add(child.Id))
                {
                    children.Add(BuildDepartment(child));
                }
            }

            var node = new DepartmentHierarchyNode
            {
                Id = department.Id,
                Name = department.Name,
                ColorHex = department.ColorHex,
                Children = children
            };
            node.IsSelected = node.Key == _selectedKey;
            return node;
        }

        private EmployeeHierarchyNode BuildEmployee(Employee employee, bool isLeader)
        {
            var node = new EmployeeHierarchyNode
            {
                Id = employee.Id,
                Name = employee.FullName,
                IsLeader = isLeader
            };
            node.IsSelected = node.Key == _selectedKey;
            return node;
        }

        private bool HasExistingParent(Department department)
        {
            return department.ParentDepartmentId is int parentId && _departmentIds.Contains(parentId);
        }

        private bool BelongsToExistingDepartment(Employee employee)
        {
            return employee.DepartmentId is int departmentId && _departmentIds.Contains(departmentId);
        }
    }
}
