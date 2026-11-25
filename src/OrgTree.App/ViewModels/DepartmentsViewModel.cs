using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.App.Services;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

internal sealed partial class DepartmentsViewModel : ObservableObject
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private bool _isEmpty;

    public DepartmentsViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public ObservableCollection<DepartmentRow> Departments { get; } = [];

    public void Load()
    {
        Departments.Clear();

        using var db = new OrgTreeContext();

        var departments = db.Departments.ToList();
        var departmentsById = departments.ToDictionary(department => department.Id);
        var employeesById = db.Employees.ToDictionary(employee => employee.Id);

        foreach (var department in departments)
        {
            var leader = department.LeaderId is int leaderId ? employeesById.GetValueOrDefault(leaderId) : null;
            var parent = department.ParentDepartmentId is int parentId ? departmentsById.GetValueOrDefault(parentId) : null;

            Departments.Add(new DepartmentRow
            {
                Id = department.Id,
                Name = department.Name,
                LeaderName = leader?.DisplayName,
                ParentDepartmentName = parent?.Name,
                ColorHex = department.ColorHex
            });
        }

        IsEmpty = Departments.Count == 0;
    }

    [RelayCommand]
    private void Create()
    {
        OpenEditor(new DepartmentEditViewModel());
    }

    [RelayCommand]
    private void Edit(DepartmentRow row)
    {
        OpenEditor(new DepartmentEditViewModel(row.Id));
    }

    [RelayCommand]
    private void Delete(DepartmentRow row)
    {
        var confirmed = _dialogService.Confirm(
            "Abteilung löschen",
            $"Möchtest du die Abteilung „{row.Name}“ wirklich löschen? Zugeordnete Mitarbeitende und Unterabteilungen bleiben erhalten, verlieren aber ihre Zuordnung.");

        if (!confirmed)
        {
            return;
        }

        using var db = new OrgTreeContext();

        var department = db.Departments.Find(row.Id);
        if (department == null)
        {
            return;
        }

        db.RemoveDepartment(department);
        db.SaveChanges();

        Load();
    }

    private void OpenEditor(DepartmentEditViewModel viewModel)
    {
        if (_dialogService.ShowEditor(viewModel))
        {
            Load();
        }
    }
}
