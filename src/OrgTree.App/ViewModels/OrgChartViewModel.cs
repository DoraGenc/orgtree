using System.Collections.ObjectModel;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public class OrgChartViewModel
{
    private const string DefaultBoxColor = "#EEEEEE";

    public ObservableCollection<OrgChartNode> Roots { get; } = new();

    public void Load()
    {
        Roots.Clear();

        using var db = new OrgTreeContext();

        var departments = db.Departments.OrderBy(d => d.Name).ToList();
        var employees = db.Employees.ToList();
        var departmentIds = departments.Select(d => d.Id).ToHashSet();

        var rootDepartments = departments
            .Where(d => d.ParentDepartmentId == null || !departmentIds.Contains(d.ParentDepartmentId.Value))
            .Where(d => departments.Any(child => child.ParentDepartmentId == d.Id))
            .ToList();

        foreach (var node in BuildNodes(rootDepartments, departments, employees, new HashSet<int>(), hasParent: false))
        {
            Roots.Add(node);
        }
    }

    private static List<OrgChartNode> BuildNodes(
        List<Department> level,
        List<Department> allDepartments,
        List<Employee> employees,
        HashSet<int> visited,
        bool hasParent)
    {
        var nodes = new List<OrgChartNode>();

        foreach (var department in level)
        {
            if (!visited.Add(department.Id))
            {
                continue;
            }

            var leader = employees.FirstOrDefault(e => e.Id == department.LeaderId);
            var node = new OrgChartNode
            {
                Name = department.Name,
                LeaderName = leader == null ? null : $"{leader.FirstName} {leader.LastName}",
                BoxColor = department.ColorHex ?? DefaultBoxColor,
                HasParent = hasParent
            };

            var children = allDepartments.Where(d => d.ParentDepartmentId == department.Id).ToList();
            node.Children.AddRange(BuildNodes(children, allDepartments, employees, visited, hasParent: true));

            nodes.Add(node);
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            nodes[i].ShowLeftLine = i > 0;
            nodes[i].ShowRightLine = i < nodes.Count - 1;
        }

        return nodes;
    }
}
