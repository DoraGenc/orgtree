using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OrgTree.Core;

namespace OrgTree.App.ViewModels;

public partial class DepartmentEditViewModel : ObservableObject
{
    private readonly int _departmentId;

    public ObservableCollection<SelectOption> Leaders { get; } = new();

    public ObservableCollection<SelectOption> ParentDepartments { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = "";

    [ObservableProperty]
    private SelectOption? _selectedLeader;

    [ObservableProperty]
    private SelectOption? _selectedParentDepartment;

    public event Action<bool>? CloseRequested;

    public DepartmentEditViewModel(int departmentId)
    {
        _departmentId = departmentId;

        using var db = new OrgTreeContext();

        var department = db.Departments.Find(departmentId)
            ?? throw new InvalidOperationException($"Abteilung {departmentId} existiert nicht.");

        var departments = db.Departments.ToList();
        var ownSubtree = CollectSubtreeIds(departments, departmentId);

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

        Name = department.Name;
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
        return !string.IsNullOrWhiteSpace(Name);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        using var db = new OrgTreeContext();

        var department = db.Departments.Find(_departmentId);
        if (department == null)
        {
            CloseRequested?.Invoke(false);
            return;
        }

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
                    foreach (var otherDepartment in db.Departments.Where(d => d.LeaderId == newLeaderId && d.Id != _departmentId))
                    {
                        otherDepartment.LeaderId = null;
                    }

                    foreach (var otherHead in db.Employees.Where(e => e.DepartmentId == _departmentId && e.DepartmentHead == true && e.Id != newLeaderId))
                    {
                        otherHead.DepartmentHead = false;
                    }

                    leader.DepartmentId = _departmentId;
                    leader.DepartmentHead = true;
                }
            }
        }

        department.Name = Name.Trim();
        department.LeaderId = newLeaderId;
        department.ParentDepartmentId = SelectedParentDepartment?.Id;

        db.SaveChanges();

        CloseRequested?.Invoke(true);
    }
}
