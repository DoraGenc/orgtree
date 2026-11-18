using OrgTree.App.ViewModels;

namespace OrgTree.App.Tests.ViewModels;

public class DepartmentsViewModelTests : DatabaseTest
{
    [Fact]
    public void EmptyDatabaseShowsEmptyState()
    {
        var viewModel = new DepartmentsViewModel();

        viewModel.Load();

        Assert.Empty(viewModel.Departments);
        Assert.True(viewModel.IsEmpty);
    }

    [Fact]
    public void RowShowsLeaderAndParentByName()
    {
        var parentId = AddDepartment("Geschäftsführung");
        var departmentId = AddDepartment("Vertrieb", parentDepartmentId: parentId, colorHex: "#55AAFF");
        AddEmployee("Müller", "Anna", departmentId: departmentId, isLeader: true);
        var viewModel = new DepartmentsViewModel();

        viewModel.Load();

        var row = viewModel.Departments.Single(department => department.Id == departmentId);
        Assert.Equal("Vertrieb", row.Name);
        Assert.Equal("Müller, Anna", row.LeaderName);
        Assert.Equal("Geschäftsführung", row.ParentDepartmentName);
        Assert.Equal("#55AAFF", row.ColorHex);
        Assert.False(viewModel.IsEmpty);
    }

    [Fact]
    public void RowLeavesMissingValuesEmpty()
    {
        var departmentId = AddDepartment("Vertrieb");
        var viewModel = new DepartmentsViewModel();

        viewModel.Load();

        var row = Assert.Single(viewModel.Departments);
        Assert.Equal(departmentId, row.Id);
        Assert.Null(row.LeaderName);
        Assert.Null(row.ParentDepartmentName);
        Assert.Null(row.ColorHex);
    }

    [Fact]
    public void LoadingTwiceDoesNotDuplicateRows()
    {
        AddDepartment("Vertrieb");
        AddDepartment("Einkauf");
        var viewModel = new DepartmentsViewModel();

        viewModel.Load();
        viewModel.Load();

        Assert.Equal(2, viewModel.Departments.Count);
    }

    [Fact]
    public void LoadingAgainShowsChangesFromDatabase()
    {
        AddDepartment("Vertrieb");
        var viewModel = new DepartmentsViewModel();
        viewModel.Load();

        AddDepartment("Einkauf");
        viewModel.Load();

        Assert.Equal(["Vertrieb", "Einkauf"], viewModel.Departments.Select(row => row.Name));
    }
}
