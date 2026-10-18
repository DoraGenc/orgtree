using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.App.Views;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class DepartmentsViewModel : ObservableObject
{
    public ObservableCollection<DepartmentRow> Departments { get; } = new();

    [ObservableProperty]
    private bool _isEmpty;

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

        IsEmpty = Departments.Count == 0;
    }

    [RelayCommand]
    private void Create()
    {
        OpenEditWindow(new DepartmentEditViewModel());
    }

    [RelayCommand]
    private void Edit(DepartmentRow row)
    {
        OpenEditWindow(new DepartmentEditViewModel(row.Id));
    }

    private void OpenEditWindow(DepartmentEditViewModel viewModel)
    {
        var window = new DepartmentEditWindow(viewModel)
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() == true)
        {
            Load();
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
