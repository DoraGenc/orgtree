using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

internal sealed partial class DepartmentEditViewModel : EditViewModel
{
    private readonly HashSet<string> _otherDepartmentNames = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameError))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorError))]
    [NotifyPropertyChangedFor(nameof(ColorPreview))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _colorHex = "";

    [ObservableProperty]
    private SelectOption? _selectedLeader;

    [ObservableProperty]
    private SelectOption? _selectedParentDepartment;

    public DepartmentEditViewModel(int? departmentId = null)
        : base(departmentId)
    {
        using var db = new OrgTreeContext();

        var departments = db.Departments.ToList();

        LoadOtherDepartmentNames(departments);
        LoadLeaderOptions(db);
        LoadParentDepartmentOptions(departments);

        if (departmentId != null)
        {
            LoadDepartment(departments, departmentId.Value);
        }
    }

    public ObservableCollection<SelectOption> Leaders { get; } = [];

    public ObservableCollection<SelectOption> ParentDepartments { get; } = [];

    public string? NameError => IsNameTaken
        ? $"Eine Abteilung mit dem Namen „{Name.Trim()}“ gibt es bereits. Bitte einen anderen Namen wählen."
        : null;

    public string? ColorError => IsColorValid
        ? null
        : "Ungültige Farbe. Erwartet: #RGB, #RRGGBB oder #AARRGGBB, z.B. #55AAFF.";

    public string? ColorPreview => IsColorValid ? NormalizedColorHex : null;

    public bool HasLeaders => Leaders.Any(leader => leader.Id != null);

    public bool HasNoLeaders => !HasLeaders;

    public bool HasParentDepartments => ParentDepartments.Any(department => department.Id != null);

    public bool HasNoParentDepartments => !HasParentDepartments;

    private bool IsNameTaken => _otherDepartmentNames.Contains(Name.Trim());

    private string? NormalizedColorHex => string.IsNullOrWhiteSpace(ColorHex) ? null : ColorHex.Trim();

    private bool IsColorValid => Department.IsValidColorHex(NormalizedColorHex);

    private void LoadOtherDepartmentNames(List<Department> departments)
    {
        foreach (var other in departments.Where(department => department.Id != Id))
        {
            _otherDepartmentNames.Add(other.Name.Trim());
        }
    }

    private void LoadLeaderOptions(OrgTreeContext db)
    {
        Leaders.Add(new SelectOption(null, DisplayTexts.PleaseSelect));

        foreach (var employee in db.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName))
        {
            Leaders.Add(new SelectOption(employee.Id, employee.DisplayName));
        }

        SelectedLeader = Leaders[0];
    }

    private void LoadParentDepartmentOptions(List<Department> departments)
    {
        HashSet<int> ownSubtree = Id == null
            ? []
            : CollectSubtreeIds(departments, Id.Value);

        ParentDepartments.Add(new SelectOption(null, DisplayTexts.PleaseSelect));

        foreach (var other in departments.Where(d => !ownSubtree.Contains(d.Id)).OrderBy(d => d.Name))
        {
            ParentDepartments.Add(new SelectOption(other.Id, other.Name));
        }

        SelectedParentDepartment = ParentDepartments[0];
    }

    private void LoadDepartment(List<Department> departments, int departmentId)
    {
        var department = departments.FirstOrDefault(d => d.Id == departmentId)
            ?? throw new InvalidOperationException($"Abteilung {departmentId} existiert nicht.");

        DisplayName = department.Name;
        Name = department.Name;
        ColorHex = department.ColorHex ?? "";
        SelectedLeader = Leaders.FirstOrDefault(option => option.Id == department.LeaderId) ?? Leaders[0];
        SelectedParentDepartment = ParentDepartments.FirstOrDefault(option => option.Id == department.ParentDepartmentId)
            ?? ParentDepartments[0];
    }

    private static HashSet<int> CollectSubtreeIds(List<Department> departments, int rootId)
    {
        var ids = new HashSet<int> { rootId };
        var pending = new Queue<int>();
        pending.Enqueue(rootId);

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var child in departments.Where(d => d.ParentDepartmentId == current))
            {
                if (ids.Add(child.Id))
                {
                    pending.Enqueue(child.Id);
                }
            }
        }

        return ids;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !IsNameTaken
            && IsColorValid;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        using var db = new OrgTreeContext();

        var department = FindOrCreateDepartment(db);
        if (department == null)
        {
            RequestClose(false);
            return;
        }

        department.Name = Name.Trim();
        department.ColorHex = NormalizedColorHex;
        department.ParentDepartmentId = SelectedParentDepartment?.Id;
        UpdateLeader(db, department, SelectedLeader?.Id);

        db.SaveChanges();

        RequestClose(true);
    }

    private Department? FindOrCreateDepartment(OrgTreeContext db)
    {
        if (Id != null)
        {
            return db.Departments.Find(Id);
        }

        var department = new Department { Name = Name.Trim() };
        db.Departments.Add(department);
        db.SaveChanges();
        return department;
    }

    private static void UpdateLeader(OrgTreeContext db, Department department, int? newLeaderId)
    {
        if (department.LeaderId == newLeaderId)
        {
            return;
        }

        var newLeader = newLeaderId == null ? null : db.Employees.Find(newLeaderId);
        if (newLeader == null)
        {
            department.LeaderId = null;
            return;
        }

        db.AssignDepartmentHead(newLeader, department);
    }
}
