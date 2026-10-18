using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class DepartmentEditViewModel : ObservableObject
{
    private readonly int? _departmentId;

    public string Heading => _departmentId == null ? "Erstellen" : "Editieren";

    public int? Id => _departmentId;

    public bool HasId => _departmentId != null;

    public string DisplayName { get; } = "";

    public ObservableCollection<SelectOption> Leaders { get; } = new();

    public ObservableCollection<SelectOption> ParentDepartments { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ColorError))]
    [NotifyPropertyChangedFor(nameof(ColorPreview))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _colorHex = "";

    private string? NormalizedColorHex => string.IsNullOrWhiteSpace(ColorHex) ? null : ColorHex.Trim();

    private bool IsColorValid => Department.IsValidColorHex(NormalizedColorHex);

    public string? ColorError => IsColorValid
        ? null
        : "Ungültige Farbe. Erwartet: #RGB, #RRGGBB oder #AARRGGBB, z.B. #55AAFF.";

    public string? ColorPreview => IsColorValid ? NormalizedColorHex : null;

    [ObservableProperty]
    private SelectOption? _selectedLeader;

    [ObservableProperty]
    private SelectOption? _selectedParentDepartment;

    public event Action<bool>? CloseRequested;

    public DepartmentEditViewModel(int? departmentId = null)
    {
        _departmentId = departmentId;

        using var db = new OrgTreeContext();

        var departments = db.Departments.ToList();
        var ownSubtree = departmentId == null
            ? new HashSet<int>()
            : CollectSubtreeIds(departments, departmentId.Value);

        Leaders.Add(new SelectOption(null, "auswählen..."));
        foreach (var employee in db.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName))
        {
            Leaders.Add(new SelectOption(employee.Id, $"{employee.LastName}, {employee.FirstName}"));
        }

        ParentDepartments.Add(new SelectOption(null, "auswählen..."));
        foreach (var other in departments.Where(d => !ownSubtree.Contains(d.Id)).OrderBy(d => d.Name))
        {
            ParentDepartments.Add(new SelectOption(other.Id, other.Name));
        }

        SelectedLeader = Leaders[0];
        SelectedParentDepartment = ParentDepartments[0];

        if (departmentId == null)
        {
            return;
        }

        var department = departments.FirstOrDefault(d => d.Id == departmentId)
            ?? throw new InvalidOperationException($"Abteilung {departmentId} existiert nicht.");

        DisplayName = department.Name;
        Name = department.Name;
        ColorHex = department.ColorHex ?? "";
        SelectedLeader = Leaders.FirstOrDefault(o => o.Id == department.LeaderId) ?? Leaders[0];
        SelectedParentDepartment = ParentDepartments.FirstOrDefault(o => o.Id == department.ParentDepartmentId) ?? ParentDepartments[0];
    }

    private static HashSet<int> CollectSubtreeIds(List<Department> departments, int rootId)
    {
        var ids = new HashSet<int> { rootId };
        var queue = new Queue<int>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in departments.Where(d => d.ParentDepartmentId == current))
            {
                if (ids.Add(child.Id))
                {
                    queue.Enqueue(child.Id);
                }
            }
        }

        return ids;
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) && IsColorValid;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        using var db = new OrgTreeContext();

        Department? department;
        if (_departmentId == null)
        {
            department = new Department { Name = Name.Trim() };
            db.Departments.Add(department);
            db.SaveChanges();
        }
        else
        {
            department = db.Departments.Find(_departmentId);
            if (department == null)
            {
                CloseRequested?.Invoke(false);
                return;
            }
        }

        var departmentId = department.Id;
        var newLeaderId = SelectedLeader?.Id;

        if (department.LeaderId != newLeaderId)
        {
            if (department.LeaderId != null)
            {
                var previousLeader = db.Employees.Find(department.LeaderId);
                if (previousLeader != null)
                {
                    previousLeader.DepartmentHead = false;
                }
            }

            if (newLeaderId != null)
            {
                var leader = db.Employees.Find(newLeaderId);
                if (leader != null)
                {
                    foreach (var otherDepartment in db.Departments.Where(d => d.LeaderId == newLeaderId && d.Id != departmentId))
                    {
                        otherDepartment.LeaderId = null;
                    }

                    foreach (var otherHead in db.Employees.Where(e => e.DepartmentId == departmentId && e.DepartmentHead == true && e.Id != newLeaderId))
                    {
                        otherHead.DepartmentHead = false;
                    }

                    leader.DepartmentId = departmentId;
                    leader.DepartmentHead = true;
                }
            }
        }

        department.Name = Name.Trim();
        department.ColorHex = NormalizedColorHex;
        department.LeaderId = newLeaderId;
        department.ParentDepartmentId = SelectedParentDepartment?.Id;

        db.SaveChanges();

        CloseRequested?.Invoke(true);
    }
}
