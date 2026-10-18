using System.Collections.ObjectModel;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public class DepartmentsViewModel
{
    public ObservableCollection<DepartmentRow> Departments { get; } = new();

    public DepartmentsViewModel()
    {
        using var db = new OrgTreeContext();

        var departments = db.Departments.ToList();
        var employees = db.Employees.ToList();

        foreach (var department in departments)
        {
            var leader = employees.FirstOrDefault(e => e.Id == department.LeaderId);
            var parent = departments.FirstOrDefault(d => d.Id == department.ParentDepartmentId);

            Departments.Add(new DepartmentRow
            {
                Id = department.Id,
                Name = department.Name,
                LeaderName = leader == null ? null : $"{leader.LastName}, {leader.FirstName}",
                ParentDepartmentName = parent?.Name,
                ColorHex = department.ColorHex
            });
        }
    }
}
