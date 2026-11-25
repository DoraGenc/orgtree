using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.App.Services;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

internal sealed partial class EmployeesViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private bool _isEmpty;

    public EmployeesViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public ObservableCollection<EmployeeRow> Employees { get; } = [];

    public void Load()
    {
        Employees.Clear();

        using var db = new OrgTreeContext();

        var departmentsById = db.Departments.ToDictionary(department => department.Id);

        foreach (var employee in db.Employees.ToList())
        {
            var department = employee.DepartmentId is int departmentId
                ? departmentsById.GetValueOrDefault(departmentId)
                : null;

            Employees.Add(new EmployeeRow
            {
                Id = employee.Id,
                Name = employee.DisplayName,
                PhoneNumber = employee.PhoneNumber,
                DepartmentName = department?.Name,
                DepartmentHead = department == null
                    ? null
                    : DisplayTexts.YesNo(department.LeaderId == employee.Id)
            });
        }

        IsEmpty = Employees.Count == 0;
    }

    [RelayCommand]
    private void Create()
    {
        OpenEditor(new EmployeeEditViewModel());
    }

    [RelayCommand]
    private void Edit(EmployeeRow row)
    {
        OpenEditor(new EmployeeEditViewModel(row.Id));
    }

    [RelayCommand]
    private void Delete(EmployeeRow row)
    {
        var confirmed = _dialogService.Confirm(
            "Mitarbeiter löschen",
            $"Möchtest du „{row.Name}“ wirklich löschen? Dieser Vorgang kann nicht rückgängig gemacht werden.");

        if (!confirmed)
        {
            return;
        }

        using var db = new OrgTreeContext();

        var employee = db.Employees.Find(row.Id);
        if (employee == null)
        {
            return;
        }

        db.Employees.Remove(employee);
        db.SaveChanges();

        Load();
    }

    private void OpenEditor(EmployeeEditViewModel viewModel)
    {
        if (_dialogService.ShowEditor(viewModel))
        {
            Load();
        }
    }
}
