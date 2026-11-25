using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

internal sealed partial class EmployeeEditViewModel : EditViewModel
{
    private readonly HashSet<int> _ledDepartmentIds = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _lastName = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _firstName = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PhoneNumberError))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _phoneNumber = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeDepartmentHead))]
    private SelectOption? _selectedDepartment;

    [ObservableProperty]
    private bool _isDepartmentHead;

    public EmployeeEditViewModel(int? employeeId = null)
        : base(employeeId)
    {
        using var db = new OrgTreeContext();

        LoadDepartmentOptions(db);

        if (employeeId != null)
        {
            LoadEmployee(db, employeeId.Value);
        }
    }

    public ObservableCollection<SelectOption> Departments { get; } = [];

    public string? PhoneNumberError => IsPhoneNumberValid
        ? null
        : "Ungültige Telefonnummer. Bitte im internationalen Format eingeben, z.B. +49 172 12345678.";

    public bool CanBeDepartmentHead => SelectedDepartment?.Id != null;

    public bool HasDepartments => Departments.Any(department => department.Id != null);

    public bool HasNoDepartments => !HasDepartments;

    private bool IsPhoneNumberValid => Employee.IsValidPhoneNumber(PhoneNumber);

    private void LoadDepartmentOptions(OrgTreeContext db)
    {
        Departments.Add(new SelectOption(null, DisplayTexts.NoDepartment));

        foreach (var department in db.Departments.OrderBy(department => department.Name))
        {
            Departments.Add(new SelectOption(department.Id, department.Name));

            if (Id != null && department.LeaderId == Id)
            {
                _ledDepartmentIds.Add(department.Id);
            }
        }

        SelectedDepartment = Departments[0];
    }

    private void LoadEmployee(OrgTreeContext db, int employeeId)
    {
        var employee = db.Employees.Find(employeeId)
            ?? throw new InvalidOperationException($"MitarbeiterIn {employeeId} existiert nicht.");

        DisplayName = employee.DisplayName;
        LastName = employee.LastName;
        FirstName = employee.FirstName;
        PhoneNumber = employee.PhoneNumber ?? "";
        SelectedDepartment = Departments.FirstOrDefault(option => option.Id == employee.DepartmentId)
            ?? Departments[0];
    }

    partial void OnSelectedDepartmentChanged(SelectOption? value)
    {
        IsDepartmentHead = value?.Id is int departmentId && _ledDepartmentIds.Contains(departmentId);
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(LastName)
            && !string.IsNullOrWhiteSpace(FirstName)
            && IsPhoneNumberValid;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        using var db = new OrgTreeContext();

        var employee = FindOrCreateEmployee(db);
        if (employee == null)
        {
            RequestClose(false);
            return;
        }

        employee.LastName = LastName.Trim();
        employee.FirstName = FirstName.Trim();
        employee.PhoneNumber = PhoneNumber;
        employee.DepartmentId = SelectedDepartment?.Id;

        var ledDepartment = IsDepartmentHead && employee.DepartmentId != null
            ? db.Departments.Find(employee.DepartmentId)
            : null;
        db.AssignDepartmentHead(employee, ledDepartment);

        db.SaveChanges();

        RequestClose(true);
    }

    private Employee? FindOrCreateEmployee(OrgTreeContext db)
    {
        if (Id != null)
        {
            return db.Employees.Find(Id);
        }

        var employee = new Employee { LastName = LastName.Trim(), FirstName = FirstName.Trim() };
        db.Employees.Add(employee);
        db.SaveChanges();
        return employee;
    }
}
