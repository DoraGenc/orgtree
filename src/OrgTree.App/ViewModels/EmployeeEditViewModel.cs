using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class EmployeeEditViewModel : ObservableObject
{
    private readonly int? _employeeId;
    private readonly HashSet<int> _ledDepartmentIds = new();

    public string Heading => _employeeId == null ? "Erstellen" : "Editieren";

    public int? Id => _employeeId;

    public bool HasId => _employeeId != null;

    public string DisplayName { get; } = "";

    public ObservableCollection<SelectOption> Departments { get; } = new();

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

    private bool IsPhoneNumberValid => Employee.IsValidPhoneNumber(PhoneNumber);

    public string? PhoneNumberError => IsPhoneNumberValid
        ? null
        : "Ungültige Telefonnummer. Bitte im internationalen Format eingeben, z.B. +49 172 12345678.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeDepartmentHead))]
    private SelectOption? _selectedDepartment;

    [ObservableProperty]
    private bool _isDepartmentHead;

    public bool CanBeDepartmentHead => SelectedDepartment?.Id != null;

    public bool HasDepartments => Departments.Any(department => department.Id != null);

    public bool HasNoDepartments => !HasDepartments;

    public event Action<bool>? CloseRequested;

    public EmployeeEditViewModel(int? employeeId = null)
    {
        _employeeId = employeeId;

        using var db = new OrgTreeContext();

        Departments.Add(new SelectOption(null, "Keine Abteilung zugeordnet"));
        foreach (var department in db.Departments.OrderBy(department => department.Name))
        {
            Departments.Add(new SelectOption(department.Id, department.Name));
            if (department.LeaderId == employeeId)
            {
                _ledDepartmentIds.Add(department.Id);
            }
        }

        SelectedDepartment = Departments[0];

        if (employeeId == null)
        {
            return;
        }

        var employee = db.Employees.Find(employeeId)
            ?? throw new InvalidOperationException($"MitarbeiterIn {employeeId} existiert nicht.");

        DisplayName = $"{employee.LastName}, {employee.FirstName}";
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

        Employee? employee;
        if (_employeeId == null)
        {
            employee = new Employee { LastName = LastName.Trim(), FirstName = FirstName.Trim() };
            db.Employees.Add(employee);
            db.SaveChanges();
        }
        else
        {
            employee = db.Employees.Find(_employeeId);
            if (employee == null)
            {
                CloseRequested?.Invoke(false);
                return;
            }
        }

        var employeeId = employee.Id;
        var newDepartmentId = SelectedDepartment?.Id;
        int? ledDepartmentId = IsDepartmentHead ? newDepartmentId : null;

        foreach (var department in db.Departments.Where(d => d.LeaderId == employeeId && d.Id != ledDepartmentId))
        {
            department.LeaderId = null;
        }

        if (ledDepartmentId != null)
        {
            var department = db.Departments.Find(ledDepartmentId);
            if (department != null)
            {
                department.LeaderId = employeeId;
            }
        }

        employee.LastName = LastName.Trim();
        employee.FirstName = FirstName.Trim();
        employee.PhoneNumber = PhoneNumber;
        employee.DepartmentId = newDepartmentId;

        db.SaveChanges();

        CloseRequested?.Invoke(true);
    }
}
