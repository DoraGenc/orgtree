using OrgTree.App.ViewModels;
using OrgTree.Core;

namespace OrgTree.App.Tests.ViewModels;

public class EmployeeEditViewModelTests : DatabaseTest
{
    [Fact]
    public void NewEmployeeStartsEmptyWithoutDepartment()
    {
        var viewModel = new EmployeeEditViewModel();

        Assert.Equal("Erstellen", viewModel.Heading);
        Assert.Equal("", viewModel.LastName);
        Assert.Equal("", viewModel.FirstName);
        Assert.Equal("", viewModel.PhoneNumber);
        Assert.Null(viewModel.SelectedDepartment?.Id);
        Assert.False(viewModel.IsDepartmentHead);
        Assert.False(viewModel.CanBeDepartmentHead);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("Müller", "")]
    [InlineData("", "Anna")]
    [InlineData("   ", "Anna")]
    public void CannotSaveWithoutBothNames(string lastName, string firstName)
    {
        var viewModel = new EmployeeEditViewModel { LastName = lastName, FirstName = firstName };

        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void CanSaveWithNamesAndWithoutPhoneNumber()
    {
        var viewModel = new EmployeeEditViewModel { LastName = "Müller", FirstName = "Anna" };

        Assert.Null(viewModel.PhoneNumberError);
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("0172 12345678")]
    [InlineData("hallo")]
    public void InvalidPhoneNumberShowsErrorAndBlocksSaving(string phoneNumber)
    {
        var viewModel = new EmployeeEditViewModel { LastName = "Müller", FirstName = "Anna", PhoneNumber = phoneNumber };

        Assert.NotNull(viewModel.PhoneNumberError);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void SavingNewEmployeeStoresTrimmedNamesAndFormattedPhoneNumber()
    {
        var viewModel = new EmployeeEditViewModel
        {
            LastName = "  Müller ",
            FirstName = " Anna",
            PhoneNumber = "+4917212345678"
        };
        bool? closedWith = null;
        viewModel.CloseRequested += saved => closedWith = saved;

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        var employee = db.Employees.Single();
        Assert.Equal("Müller", employee.LastName);
        Assert.Equal("Anna", employee.FirstName);
        Assert.Equal("+49 172 12345678", employee.PhoneNumber);
        Assert.Null(employee.DepartmentId);
        Assert.True(closedWith);
    }

    [Fact]
    public void SavingWithEmptyPhoneNumberStoresNull()
    {
        var viewModel = new EmployeeEditViewModel { LastName = "Müller", FirstName = "Anna", PhoneNumber = "  " };

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        Assert.Null(db.Employees.Single().PhoneNumber);
    }

    [Fact]
    public void SelectingDepartmentAllowsDepartmentHead()
    {
        var departmentId = AddDepartment("Vertrieb");
        var viewModel = new EmployeeEditViewModel();

        viewModel.SelectedDepartment = viewModel.Departments.Single(option => option.Id == departmentId);

        Assert.True(viewModel.CanBeDepartmentHead);
    }

    [Fact]
    public void SelectingDepartmentForNewEmployeeDoesNotTickDepartmentHead()
    {
        var departmentId = AddDepartment("Vertrieb");
        var viewModel = new EmployeeEditViewModel();

        viewModel.SelectedDepartment = viewModel.Departments.Single(option => option.Id == departmentId);

        Assert.False(viewModel.IsDepartmentHead);
    }

    [Fact]
    public void SavingAsDepartmentHeadSetsDepartmentAndLeader()
    {
        var departmentId = AddDepartment("Vertrieb");
        var viewModel = new EmployeeEditViewModel { LastName = "Müller", FirstName = "Anna" };
        viewModel.SelectedDepartment = viewModel.Departments.Single(option => option.Id == departmentId);
        viewModel.IsDepartmentHead = true;

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        var employee = db.Employees.Single();
        Assert.Equal(departmentId, employee.DepartmentId);
        Assert.Equal(employee.Id, db.Departments.Find(departmentId)!.LeaderId);
    }

    [Fact]
    public void EditingExistingEmployeeLoadsAllValues()
    {
        var departmentId = AddDepartment("Vertrieb");
        var employeeId = AddEmployee("Müller", "Anna", "+4917212345678", departmentId, isLeader: true);

        var viewModel = new EmployeeEditViewModel(employeeId);

        Assert.Equal("Editieren", viewModel.Heading);
        Assert.Equal("Müller, Anna", viewModel.DisplayName);
        Assert.Equal("Müller", viewModel.LastName);
        Assert.Equal("Anna", viewModel.FirstName);
        Assert.Equal("+49 172 12345678", viewModel.PhoneNumber);
        Assert.Equal(departmentId, viewModel.SelectedDepartment?.Id);
        Assert.True(viewModel.IsDepartmentHead);
    }

    [Fact]
    public void UntickingDepartmentHeadRemovesLeaderOnSave()
    {
        var departmentId = AddDepartment("Vertrieb");
        var employeeId = AddEmployee("Müller", "Anna", null, departmentId, isLeader: true);
        var viewModel = new EmployeeEditViewModel(employeeId) { IsDepartmentHead = false };

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        Assert.Null(db.Departments.Find(departmentId)!.LeaderId);
        Assert.Equal(departmentId, db.Employees.Find(employeeId)!.DepartmentId);
    }

    [Fact]
    public void EditingDoesNotCreateSecondEmployee()
    {
        var employeeId = AddEmployee("Müller", "Anna", null, null, isLeader: false);
        var viewModel = new EmployeeEditViewModel(employeeId) { FirstName = "Annika" };

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        Assert.Equal("Annika", db.Employees.Single().FirstName);
    }
}
