using FlaUI.Core.AutomationElements;
using OrgTree.Core;

namespace OrgTree.App.UiTests;

public class DepartmentUiTests : AppTest
{
    [Fact]
    public void CreatedDepartmentAppearsInTable()
    {
        OpenArea("Abteilungen");
        ById(MainWindow, "CreateButton").AsButton().Invoke();
        var dialog = WaitForDialog();

        ById(dialog, "NameField").AsTextBox().Text = "Vertrieb";
        ById(dialog, "ColorField").AsTextBox().Text = "#55AAFF";
        ById(dialog, "SaveButton").AsButton().Invoke();
        WaitUntilDialogClosed();

        var row = Assert.Single(Retry(() => RowTexts("DepartmentsGrid")));
        Assert.Contains("Vertrieb", row);
    }

    [Fact]
    public void DuplicateNameShowsErrorAndDisablesSave()
    {
        Seed(db => db.Departments.Add(new Department { Name = "Vertrieb" }));
        OpenArea("Abteilungen");
        ById(MainWindow, "CreateButton").AsButton().Invoke();
        var dialog = WaitForDialog();

        ById(dialog, "NameField").AsTextBox().Text = "vertrieb";

        Assert.Contains("gibt es bereits", ById(dialog, "NameError").Name);
        Assert.True(Eventually(() => !ById(dialog, "SaveButton").IsEnabled));
    }

    [Fact]
    public void InvalidColorShowsErrorAndDisablesSave()
    {
        OpenArea("Abteilungen");
        ById(MainWindow, "CreateButton").AsButton().Invoke();
        var dialog = WaitForDialog();

        ById(dialog, "NameField").AsTextBox().Text = "Vertrieb";
        ById(dialog, "ColorField").AsTextBox().Text = "blau";

        Assert.Contains("Ungültige Farbe", ById(dialog, "ColorError").Name);
        Assert.True(Eventually(() => !ById(dialog, "SaveButton").IsEnabled));
    }
}
