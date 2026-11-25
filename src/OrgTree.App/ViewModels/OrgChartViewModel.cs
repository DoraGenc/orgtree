using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

internal sealed partial class OrgChartViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isEmpty;

    public ObservableCollection<OrgChartNode> Roots { get; } = [];

    public void Load()
    {
        Roots.Clear();

        using var db = new OrgTreeContext();

        var departments = db.Departments.OrderBy(department => department.Name).ToList();
        var employeesById = db.Employees.ToDictionary(employee => employee.Id);

        var builder = new ChartBuilder(departments, employeesById);
        foreach (var root in builder.BuildRoots())
        {
            Roots.Add(root);
        }

        IsEmpty = Roots.Count == 0;
    }

    private sealed class ChartBuilder
    {
        private readonly List<Department> _departments;
        private readonly HashSet<int> _departmentIds;
        private readonly ILookup<int?, Department> _departmentsByParent;
        private readonly Dictionary<int, Employee> _employeesById;
        private readonly HashSet<int> _visited = [];

        public ChartBuilder(List<Department> departments, Dictionary<int, Employee> employeesById)
        {
            _departments = departments;
            _departmentIds = departments.Select(department => department.Id).ToHashSet();
            _departmentsByParent = departments.ToLookup(department => department.ParentDepartmentId);
            _employeesById = employeesById;
        }

        public List<OrgChartNode> BuildRoots()
        {
            var roots = _departments
                .Where(department => !HasExistingParent(department))
                .Where(department => _departmentsByParent[department.Id].Any())
                .ToList();

            return BuildLevel(roots, hasParent: false);
        }

        private List<OrgChartNode> BuildLevel(List<Department> level, bool hasParent)
        {
            var unvisited = new List<Department>();
            foreach (var department in level)
            {
                if (_visited.Add(department.Id))
                {
                    unvisited.Add(department);
                }
            }

            var nodes = new List<OrgChartNode>(unvisited.Count);
            for (var i = 0; i < unvisited.Count; i++)
            {
                nodes.Add(BuildNode(unvisited[i], hasParent, isFirst: i == 0, isLast: i == unvisited.Count - 1));
            }

            return nodes;
        }

        private OrgChartNode BuildNode(Department department, bool hasParent, bool isFirst, bool isLast)
        {
            var leader = department.LeaderId is int leaderId ? _employeesById.GetValueOrDefault(leaderId) : null;
            var children = _departmentsByParent[department.Id].ToList();

            return new OrgChartNode
            {
                Name = department.Name,
                LeaderName = leader?.FullName,
                BoxColor = department.ColorHex ?? DisplayTexts.DefaultDepartmentColor,
                HasParent = hasParent,
                ShowLeftLine = !isFirst,
                ShowRightLine = !isLast,
                Children = BuildLevel(children, hasParent: true)
            };
        }

        private bool HasExistingParent(Department department)
        {
            return department.ParentDepartmentId is int parentId && _departmentIds.Contains(parentId);
        }
    }
}
