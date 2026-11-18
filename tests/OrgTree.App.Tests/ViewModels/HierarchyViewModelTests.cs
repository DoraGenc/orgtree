using OrgTree.App.ViewModels;

namespace OrgTree.App.Tests.ViewModels;

public class HierarchyViewModelTests : DatabaseTest
{
    [Fact]
    public void EmptyDatabaseShowsEmptyState()
    {
        var viewModel = new HierarchyViewModel();

        viewModel.Load();

        Assert.Empty(viewModel.Roots);
        Assert.True(viewModel.IsEmpty);
        Assert.False(viewModel.ShowSelectionHint);
    }

    [Fact]
    public void TopLevelDepartmentsAreSortedByName()
    {
        AddDepartment("Vertrieb");
        AddDepartment("Einkauf");
        AddDepartment("IT");

        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        Assert.Equal(["Einkauf", "IT", "Vertrieb"], viewModel.Roots.Select(node => node.Name));
        Assert.False(viewModel.IsEmpty);
        Assert.True(viewModel.ShowSelectionHint);
    }

    [Fact]
    public void SubDepartmentsAndEmployeesAreNestedUnderTheirDepartment()
    {
        var rootId = AddDepartment("Geschäftsführung");
        var salesId = AddDepartment("Vertrieb", parentDepartmentId: rootId);
        AddEmployee("Müller", "Anna", departmentId: rootId);
        AddEmployee("Schmidt", "Bea", departmentId: salesId);

        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        var root = Assert.Single(viewModel.Roots);
        Assert.Equal("Geschäftsführung", root.Name);
        Assert.Collection(
            root.Children,
            node => Assert.Equal("Anna Müller", Assert.IsType<EmployeeHierarchyNode>(node).Name),
            node =>
            {
                var sales = Assert.IsType<DepartmentHierarchyNode>(node);
                Assert.Equal("Vertrieb", sales.Name);
                Assert.Equal("Bea Schmidt", Assert.IsType<EmployeeHierarchyNode>(Assert.Single(sales.Children)).Name);
            });
    }

    [Fact]
    public void LeaderIsListedFirstAndMarked()
    {
        var departmentId = AddDepartment("Vertrieb");
        AddEmployee("Albers", "Carl", departmentId: departmentId);
        AddEmployee("Zander", "Anna", departmentId: departmentId, isLeader: true);

        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        var employees = viewModel.Roots.Single().Children.Cast<EmployeeHierarchyNode>().ToList();
        Assert.Equal(["Anna Zander", "Carl Albers"], employees.Select(employee => employee.Name));
        Assert.True(employees[0].IsLeader);
        Assert.False(employees[1].IsLeader);
    }

    [Fact]
    public void EmployeesWithoutDepartmentAreCollectedInLastNode()
    {
        AddDepartment("Vertrieb");
        AddEmployee("Müller", "Anna");

        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        var unassigned = viewModel.Roots.Last();
        Assert.Equal("Keine Abteilung zugeordnet", unassigned.Name);
        Assert.Null(unassigned.Id);
        Assert.False(unassigned.IsDepartment);
        Assert.Equal("Anna Müller", Assert.IsType<EmployeeHierarchyNode>(Assert.Single(unassigned.Children)).Name);
    }

    [Fact]
    public void SelectingEmployeeShowsAllDetails()
    {
        var departmentId = AddDepartment("Vertrieb");
        var employeeId = AddEmployee("Müller", "Anna", "+4917212345678", departmentId, isLeader: true);
        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        viewModel.Select(FindEmployee(viewModel, employeeId));

        var details = Assert.IsType<EmployeeDetails>(viewModel.Detail);
        Assert.Equal(employeeId, details.Id);
        Assert.Equal("Müller, Anna", details.DisplayName);
        Assert.Equal("Müller", details.LastName);
        Assert.Equal("Anna", details.FirstName);
        Assert.Equal("+49 172 12345678", details.PhoneNumber);
        Assert.Equal("Vertrieb", details.DepartmentName);
        Assert.Equal("Ja", details.DepartmentHead);
        Assert.False(viewModel.ShowSelectionHint);
    }

    [Fact]
    public void EmployeeDetailsLeaveMissingValuesEmpty()
    {
        var employeeId = AddEmployee("Müller", "Anna");
        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        viewModel.Select(FindEmployee(viewModel, employeeId));

        var details = Assert.IsType<EmployeeDetails>(viewModel.Detail);
        Assert.Null(details.PhoneNumber);
        Assert.Null(details.DepartmentName);
        Assert.Equal("Nein", details.DepartmentHead);
    }

    [Fact]
    public void SelectingDepartmentShowsAllDetails()
    {
        var parentId = AddDepartment("Geschäftsführung");
        var departmentId = AddDepartment("Vertrieb", parentDepartmentId: parentId, colorHex: "#55AAFF");
        AddEmployee("Müller", "Anna", departmentId: departmentId, isLeader: true);
        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        viewModel.Select(FindDepartment(viewModel, departmentId));

        var details = Assert.IsType<DepartmentDetails>(viewModel.Detail);
        Assert.Equal(departmentId, details.Id);
        Assert.Equal("Vertrieb", details.Name);
        Assert.Equal("#55AAFF", details.ColorHex);
        Assert.Equal("Müller, Anna", details.LeaderName);
        Assert.Equal("Geschäftsführung", details.ParentDepartmentName);
    }

    [Fact]
    public void DepartmentDetailsLeaveMissingValuesEmpty()
    {
        var departmentId = AddDepartment("Vertrieb");
        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        viewModel.Select(FindDepartment(viewModel, departmentId));

        var details = Assert.IsType<DepartmentDetails>(viewModel.Detail);
        Assert.Null(details.ColorHex);
        Assert.Null(details.LeaderName);
        Assert.Null(details.ParentDepartmentName);
    }

    [Fact]
    public void SelectingUnassignedNodeShowsNoDetails()
    {
        AddEmployee("Müller", "Anna");
        var viewModel = new HierarchyViewModel();
        viewModel.Load();

        viewModel.Select(viewModel.Roots.Single());

        Assert.Null(viewModel.Detail);
        Assert.True(viewModel.ShowSelectionHint);
    }

    [Fact]
    public void ReloadingKeepsSelectedNodeSelected()
    {
        var departmentId = AddDepartment("Vertrieb");
        var employeeId = AddEmployee("Müller", "Anna", departmentId: departmentId);
        var viewModel = new HierarchyViewModel();
        viewModel.Load();
        viewModel.Select(FindEmployee(viewModel, employeeId));

        viewModel.Load();

        Assert.True(FindEmployee(viewModel, employeeId).IsSelected);
        Assert.False(FindDepartment(viewModel, departmentId).IsSelected);
    }

    private static EmployeeHierarchyNode FindEmployee(HierarchyViewModel viewModel, int employeeId)
    {
        return AllNodes(viewModel).OfType<EmployeeHierarchyNode>().Single(node => node.Id == employeeId);
    }

    private static DepartmentHierarchyNode FindDepartment(HierarchyViewModel viewModel, int departmentId)
    {
        return AllNodes(viewModel).OfType<DepartmentHierarchyNode>().Single(node => node.Id == departmentId);
    }

    private static IEnumerable<HierarchyNode> AllNodes(HierarchyViewModel viewModel)
    {
        var pending = new Stack<HierarchyNode>(viewModel.Roots);
        while (pending.Count > 0)
        {
            var node = pending.Pop();
            yield return node;

            if (node is DepartmentHierarchyNode department)
            {
                foreach (var child in department.Children)
                {
                    pending.Push(child);
                }
            }
        }
    }
}
