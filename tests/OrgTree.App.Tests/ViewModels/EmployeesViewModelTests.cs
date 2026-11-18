using OrgTree.App.ViewModels;

namespace OrgTree.App.Tests.ViewModels;

public class EmployeesViewModelTests : DatabaseTest
{
    [Fact]
    public void EmptyDatabaseShowsEmptyState()
    {
        var viewModel = new EmployeesViewModel();

        viewModel.Load();

        Assert.Empty(viewModel.Employees);
        Assert.True(viewModel.IsEmpty);
    }

    [Fact]
    public void RowShowsNamePhoneAndDepartment()
    {
        var departmentId = AddDepartment("Vertrieb");
        var employeeId = AddEmployee("Müller", "Anna", "+4917212345678", departmentId);
        var viewModel = new EmployeesViewModel();

        viewModel.Load();

        var row = Assert.Single(viewModel.Employees);
        Assert.Equal(employeeId, row.Id);
        Assert.Equal("Müller, Anna", row.Name);
        Assert.Equal("+49 172 12345678", row.PhoneNumber);
        Assert.Equal("Vertrieb", row.DepartmentName);
        Assert.False(viewModel.IsEmpty);
    }

    [Fact]
    public void DepartmentHeadShowsJaOrNein()
    {
        var departmentId = AddDepartment("Vertrieb");
        var leaderId = AddEmployee("Müller", "Anna", departmentId: departmentId, isLeader: true);
        var memberId = AddEmployee("Schmidt", "Bea", departmentId: departmentId);
        var viewModel = new EmployeesViewModel();

        viewModel.Load();

        Assert.Equal("Ja", viewModel.Employees.Single(row => row.Id == leaderId).DepartmentHead);
        Assert.Equal("Nein", viewModel.Employees.Single(row => row.Id == memberId).DepartmentHead);
    }

    [Fact]
    public void EmployeeWithoutDepartmentLeavesDepartmentColumnsEmpty()
    {
        AddEmployee("Müller", "Anna");
        var viewModel = new EmployeesViewModel();

        viewModel.Load();

        var row = Assert.Single(viewModel.Employees);
        Assert.Null(row.PhoneNumber);
        Assert.Null(row.DepartmentName);
        Assert.Null(row.DepartmentHead);
    }

    [Fact]
    public void LoadingTwiceDoesNotDuplicateRows()
    {
        AddEmployee("Müller", "Anna");
        AddEmployee("Schmidt", "Bea");
        var viewModel = new EmployeesViewModel();

        viewModel.Load();
        viewModel.Load();

        Assert.Equal(2, viewModel.Employees.Count);
    }
}
