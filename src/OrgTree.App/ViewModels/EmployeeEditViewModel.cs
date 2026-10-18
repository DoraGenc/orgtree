using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class EmployeeEditViewModel : ObservableObject
{
    private readonly int _employeeId;

    public ObservableCollection<SelectOption> Departments { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _lastName = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _firstName = "";

    [ObservableProperty]
    private string _phoneNumber = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeDepartmentHead))]
    private SelectOption? _selectedDepartment;

    [ObservableProperty]
    private bool _isDepartmentHead;

    public bool CanBeDepartmentHead => SelectedDepartment?.Id != null;

    public bool HasDepartments => Departments.Count > 1;

    public bool HasNoDepartments => !HasDepartments;

    public event Action<bool>? CloseRequested;

    public EmployeeEditViewModel(int employeeId)
    {
        _employeeId = employeeId;

        using var db = new OrgTreeContext();

        var employee = db.Employees.Find(employeeId)
            ?? throw new InvalidOperationException($"MitarbeiterIn {employeeId} existiert nicht.");

        Departments.Add(new SelectOption(null, "auswählen..."));
        foreach (var department in db.Departments.OrderBy(d => d.Name))
        {
            Departments.Add(new SelectOption(department.Id, department.Name));
        }

        LastName = employee.LastName;
        FirstName = employee.FirstName;
        PhoneNumber = employee.PhoneNumber ?? "";
        SelectedDepartment = Departments.FirstOrDefault(o => o.Id == employee.DepartmentId) ?? Departments[0];
        IsDepartmentHead = employee.DepartmentHead == true && CanBeDepartmentHead;
    }

    partial void OnSelectedDepartmentChanged(SelectOption? value)
    {
        if (value?.Id == null)
        {
            IsDepartmentHead = false;
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(FirstName);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        using var db = new OrgTreeContext();

        var employee = db.Employees.Find(_employeeId);
        if (employee == null)
        {
            CloseRequested?.Invoke(false);
            return;
        }

        var newDepartmentId = SelectedDepartment?.Id;
        var ledDepartmentId = IsDepartmentHead ? newDepartmentId : null;

        foreach (var department in db.Departments.Where(d => d.LeaderId == _employeeId && d.Id != ledDepartmentId))
        {
            department.LeaderId = null;
        }

        if (ledDepartmentId != null)
        {
            var department = db.Departments.Find(ledDepartmentId);
            if (department != null)
            {
                foreach (var other in db.Employees.Where(e => e.DepartmentId == department.Id && e.Id != _employeeId && e.DepartmentHead == true))
                {
                    other.DepartmentHead = false;
                }

                department.LeaderId = _employeeId;
            }
        }

        employee.LastName = LastName.Trim();
        employee.FirstName = FirstName.Trim();
        employee.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim();
        employee.DepartmentId = newDepartmentId;
        employee.DepartmentHead = IsDepartmentHead;

        db.SaveChanges();

        CloseRequested?.Invoke(true);
    }
}
