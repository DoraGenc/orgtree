using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using OrgTree.App.Views;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class EmployeesViewModel
{
    public ObservableCollection<EmployeeRow> Employees { get; } = new();

    public void Load()
    {
        Employees.Clear();

        using var db = new OrgTreeContext();

        var employees = db.Employees.ToList();
        var departments = db.Departments.ToList();

        foreach (var employee in employees)
        {
            var department = departments.FirstOrDefault(d => d.Id == employee.DepartmentId);

            Employees.Add(new EmployeeRow
            {
                Id = employee.Id,
                Name = $"{employee.LastName}, {employee.FirstName}",
                PhoneNumber = employee.PhoneNumber,
                DepartmentName = department?.Name,
                DepartmentHead = employee.DepartmentHead switch
                {
                    true => "Ja",
                    false => "Nein",
                    null => null
                }
            });
        }
    }

    [RelayCommand]
    private void Create()
    {
        OpenEditWindow(new EmployeeEditViewModel());
    }

    [RelayCommand]
    private void Edit(EmployeeRow row)
    {
        OpenEditWindow(new EmployeeEditViewModel(row.Id));
    }

    private void OpenEditWindow(EmployeeEditViewModel viewModel)
    {
        var window = new EmployeeEditWindow(viewModel)
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() == true)
        {
            Load();
        }
    }

    [RelayCommand]
    private void Delete(EmployeeRow row)
    {
        using var db = new OrgTreeContext();

        var employee = db.Employees.Find(row.Id);
        if (employee == null)
        {
            return;
        }

        foreach (var department in db.Departments.Where(d => d.LeaderId == row.Id))
        {
            department.LeaderId = null;
        }

        db.Employees.Remove(employee);
        db.SaveChanges();

        Load();
    }
}
