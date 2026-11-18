using OrgTree.App.ViewModels;
using OrgTree.Core;

namespace OrgTree.App.Tests.ViewModels;

public class DepartmentEditViewModelTests : DatabaseTest
{
    [Fact]
    public void NewDepartmentStartsEmpty()
    {
        var viewModel = new DepartmentEditViewModel();

        Assert.Equal("Erstellen", viewModel.Heading);
        Assert.Equal("", viewModel.Name);
        Assert.Equal("", viewModel.ColorHex);
        Assert.Null(viewModel.SelectedLeader?.Id);
        Assert.Null(viewModel.SelectedParentDepartment?.Id);
        Assert.Null(viewModel.ColorError);
        Assert.Null(viewModel.NameError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CannotSaveWithoutName(string name)
    {
        var viewModel = new DepartmentEditViewModel { Name = name };

        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void NewDepartmentInEmptyDatabaseHasNoNameError()
    {
        var viewModel = new DepartmentEditViewModel { Name = "Vertrieb" };

        Assert.Null(viewModel.NameError);
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("Vertrieb")]
    [InlineData("vertrieb")]
    [InlineData("  VERTRIEB ")]
    public void DuplicateNameShowsErrorAndBlocksSaving(string name)
    {
        AddDepartment("Vertrieb");

        var viewModel = new DepartmentEditViewModel { Name = name };

        Assert.NotNull(viewModel.NameError);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void KeepingOwnNameWhileEditingIsAllowed()
    {
        var departmentId = AddDepartment("Vertrieb");

        var viewModel = new DepartmentEditViewModel(departmentId);

        Assert.Null(viewModel.NameError);
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void RenamingToAnotherExistingNameWhileEditingIsBlocked()
    {
        AddDepartment("Einkauf");
        var departmentId = AddDepartment("Vertrieb");

        var viewModel = new DepartmentEditViewModel(departmentId) { Name = "einkauf" };

        Assert.NotNull(viewModel.NameError);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Theory]
    [InlineData("55AAFF")]
    [InlineData("#GGG")]
    public void InvalidColorShowsErrorAndBlocksSaving(string colorHex)
    {
        var viewModel = new DepartmentEditViewModel { Name = "Vertrieb", ColorHex = colorHex };

        Assert.NotNull(viewModel.ColorError);
        Assert.Null(viewModel.ColorPreview);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void ValidColorShowsTrimmedPreview()
    {
        var viewModel = new DepartmentEditViewModel { Name = "Vertrieb", ColorHex = "  #55AAFF " };

        Assert.Null(viewModel.ColorError);
        Assert.Equal("#55AAFF", viewModel.ColorPreview);
    }

    [Fact]
    public void SavingNewDepartmentStoresTrimmedValues()
    {
        var viewModel = new DepartmentEditViewModel { Name = "  Vertrieb ", ColorHex = " #55AAFF " };
        bool? closedWith = null;
        viewModel.CloseRequested += saved => closedWith = saved;

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        var department = db.Departments.Single();
        Assert.Equal("Vertrieb", department.Name);
        Assert.Equal("#55AAFF", department.ColorHex);
        Assert.Null(department.LeaderId);
        Assert.Null(department.ParentDepartmentId);
        Assert.True(closedWith);
    }

    [Fact]
    public void SavingWithEmptyColorStoresNull()
    {
        var viewModel = new DepartmentEditViewModel { Name = "Vertrieb", ColorHex = "   " };

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        Assert.Null(db.Departments.Single().ColorHex);
    }

    [Fact]
    public void LeadersAreListedByLastNameThenFirstName()
    {
        AddEmployee("Schmidt", "Bea");
        AddEmployee("Müller", "Carl");
        AddEmployee("Müller", "Anna");

        var viewModel = new DepartmentEditViewModel();

        Assert.Equal(
            ["auswählen...", "Müller, Anna", "Müller, Carl", "Schmidt, Bea"],
            viewModel.Leaders.Select(option => option.Name));
    }

    [Fact]
    public void SavingWithLeaderMovesLeaderIntoDepartment()
    {
        var otherDepartmentId = AddDepartment("Einkauf");
        var employeeId = AddEmployee("Müller", "Anna", departmentId: otherDepartmentId);
        var viewModel = new DepartmentEditViewModel { Name = "Vertrieb" };
        viewModel.SelectedLeader = viewModel.Leaders.Single(option => option.Id == employeeId);

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        var department = db.Departments.Single(d => d.Name == "Vertrieb");
        Assert.Equal(employeeId, department.LeaderId);
        Assert.Equal(department.Id, db.Employees.Find(employeeId)!.DepartmentId);
    }

    [Fact]
    public void NewLeaderIsRemovedAsLeaderOfPreviousDepartment()
    {
        var previousDepartmentId = AddDepartment("Einkauf");
        var employeeId = AddEmployee("Müller", "Anna", departmentId: previousDepartmentId, isLeader: true);
        var viewModel = new DepartmentEditViewModel { Name = "Vertrieb" };
        viewModel.SelectedLeader = viewModel.Leaders.Single(option => option.Id == employeeId);

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        Assert.Null(db.Departments.Find(previousDepartmentId)!.LeaderId);
    }

    [Fact]
    public void ParentDepartmentOptionsExcludeOwnSubtree()
    {
        var rootId = AddDepartment("Geschäftsführung");
        var childId = AddDepartment("Vertrieb", parentDepartmentId: rootId);
        AddDepartment("Vertrieb Nord", parentDepartmentId: childId);
        var unrelatedId = AddDepartment("IT");

        var viewModel = new DepartmentEditViewModel(rootId);

        Assert.Equal(
            [null, unrelatedId],
            viewModel.ParentDepartments.Select(option => option.Id));
    }

    [Fact]
    public void EditingExistingDepartmentLoadsAllValues()
    {
        var parentId = AddDepartment("Geschäftsführung");
        var departmentId = AddDepartment("Vertrieb", parentDepartmentId: parentId, colorHex: "#55AAFF");
        var leaderId = AddEmployee("Müller", "Anna", departmentId: departmentId, isLeader: true);

        var viewModel = new DepartmentEditViewModel(departmentId);

        Assert.Equal("Editieren", viewModel.Heading);
        Assert.Equal("Vertrieb", viewModel.DisplayName);
        Assert.Equal("Vertrieb", viewModel.Name);
        Assert.Equal("#55AAFF", viewModel.ColorHex);
        Assert.Equal(leaderId, viewModel.SelectedLeader?.Id);
        Assert.Equal(parentId, viewModel.SelectedParentDepartment?.Id);
    }

    [Fact]
    public void EditingDoesNotCreateSecondDepartment()
    {
        var departmentId = AddDepartment("Vertrieb");
        var viewModel = new DepartmentEditViewModel(departmentId) { Name = "Vertrieb & Marketing" };

        viewModel.SaveCommand.Execute(null);

        using var db = new OrgTreeContext();
        Assert.Equal("Vertrieb & Marketing", db.Departments.Single().Name);
    }
}
