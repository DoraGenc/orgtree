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
    [InlineData(AppPage.Start, 0)]
    [InlineData(AppPage.Departments, 1)]
    [InlineData(AppPage.Employees, 2)]
    [InlineData(AppPage.Hierarchy, 3)]
    [InlineData(AppPage.OrgChart, 4)]
    public void NavigateSelectsMatchingTab(AppPage page, int expectedTabIndex)
    {
        var viewModel = new MainViewModel();

        viewModel.NavigateCommand.Execute(page);

        Assert.Equal(expectedTabIndex, viewModel.SelectedTabIndex);
    }
}
