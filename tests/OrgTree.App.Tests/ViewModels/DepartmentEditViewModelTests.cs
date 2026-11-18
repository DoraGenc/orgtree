using OrgTree.App.ViewModels;
using OrgTree.Core;

namespace OrgTree.App.Tests.ViewModels;

public class DepartmentEditViewModelTests : DatabaseTest
{
    [Fact]
    public void NewDepartmentInEmptyDatabaseHasNoNameError()
    {
        var viewModel = new DepartmentEditViewModel();
        viewModel.Name = "Vertrieb";

        Assert.Null(viewModel.NameError);
        Assert.True(viewModel.SaveCommand.CanExecute(null));
    }

    [Fact]
    public void DuplicateNameShowsErrorAndBlocksSaving()
    {
        using (var db = new OrgTreeContext())
        {
            db.Departments.Add(new Department { Name = "Vertrieb" });
            db.SaveChanges();
        }

        var viewModel = new DepartmentEditViewModel();
        viewModel.Name = "vertrieb";

        Assert.NotNull(viewModel.NameError);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }
}
