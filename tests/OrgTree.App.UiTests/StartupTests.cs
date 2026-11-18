using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace OrgTree.App.UiTests;

public class StartupTests : AppTest
{
    [Fact]
    public void AppStartsWithMainWindow()
    {
        Assert.Equal("orgtree", MainWindow.Title);
    }

    [Fact]
    public void StartPageOffersAllAreasAndHidesTabs()
    {
        var buttonNames = MainWindow
            .FindAllDescendants(condition => condition.ByControlType(ControlType.Button))
            .Select(button => button.Name)
            .ToList();

        Assert.Contains("Abteilungen", buttonNames);
        Assert.Contains("MitarbeiterInnen", buttonNames);
        Assert.Contains("Baumansicht", buttonNames);
        Assert.Contains("Organigramm", buttonNames);
        Assert.Empty(MainWindow.FindAllDescendants(condition => condition.ByControlType(ControlType.TabItem)));
    }

    [Fact]
    public void ClickingAreaOnStartPageShowsTabs()
    {
        MainWindow
            .FindFirstDescendant(condition => condition.ByControlType(ControlType.Button).And(condition.ByName("MitarbeiterInnen")))!
            .AsButton()
            .Invoke();

        var tabNames = Retry(() => MainWindow
            .FindAllDescendants(condition => condition.ByControlType(ControlType.TabItem))
            .Select(tab => tab.Name)
            .ToList());

        Assert.Contains("Abteilungen", tabNames);
        Assert.Contains("MitarbeiterInnen", tabNames);
        Assert.Contains("Baumansicht", tabNames);
        Assert.Contains("Organigramm", tabNames);
    }
}
