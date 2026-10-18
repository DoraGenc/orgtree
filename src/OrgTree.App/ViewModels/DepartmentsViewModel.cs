using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class DepartmentsViewModel
{
    public ObservableCollection<DepartmentRow> Departments { get; } = new();

    public void Load()
    {
        Departments.Clear();

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

    [RelayCommand]
    private void Delete(DepartmentRow row)
    {
        using var db = new OrgTreeContext();

        var department = db.Departments.Find(row.Id);
        if (department == null)
        {
            return;
        }

        foreach (var child in db.Departments.Where(d => d.ParentDepartmentId == row.Id))
        {
            child.ParentDepartmentId = null;
        }

        foreach (var employee in db.Employees.Where(e => e.DepartmentId == row.Id))
        {
            employee.DepartmentId = null;
        }

        db.Departments.Remove(department);
        db.SaveChanges();

        Load();
    }
}
