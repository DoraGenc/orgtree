using OrgTree.App.ViewModels;

namespace OrgTree.App.Tests.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void StartsOnStartPage()
    {
        var viewModel = new MainViewModel();

        Assert.Equal((int)AppPage.Start, viewModel.SelectedTabIndex);
    }

    [Theory]
    [InlineData(nameof(AppPage.Start), 0)]
    [InlineData(nameof(AppPage.Departments), 1)]
    [InlineData(nameof(AppPage.Employees), 2)]
    [InlineData(nameof(AppPage.Hierarchy), 3)]
    [InlineData(nameof(AppPage.OrgChart), 4)]
    public void NavigateSelectsMatchingTab(string pageName, int expectedTabIndex)
    {
        var viewModel = new MainViewModel();

        viewModel.NavigateCommand.Execute(Enum.Parse<AppPage>(pageName));

        Assert.Equal(expectedTabIndex, viewModel.SelectedTabIndex);
    }
}
