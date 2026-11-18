using FlaUI.Core.AutomationElements;
using OrgTree.Core;

namespace OrgTree.App.UiTests;

public class EmployeeUiTests : AppTest
{
    [Fact]
    public void CreatedEmployeeAppearsInTableWithFormattedPhoneNumber()
    {
        OpenArea("MitarbeiterInnen");
        ById(MainWindow, "CreateButton").AsButton().Invoke();
        var dialog = WaitForDialog();

        ById(dialog, "LastNameField").AsTextBox().Text = "Müller";
        ById(dialog, "FirstNameField").AsTextBox().Text = "Anna";
        ById(dialog, "PhoneNumberField").AsTextBox().Text = "+4917212345678";
        ById(dialog, "SaveButton").AsButton().Invoke();
        WaitUntilDialogClosed();

        var rows = Retry(() => RowTexts("EmployeesGrid"));
        var row = Assert.Single(rows);
        Assert.Contains("Müller, Anna", row);
        Assert.Contains("+49 172 12345678", row);
    }

    [Fact]
    public void InvalidPhoneNumberShowsErrorAndDisablesSave()
    {
        OpenArea("MitarbeiterInnen");
        ById(MainWindow, "CreateButton").AsButton().Invoke();
        var dialog = WaitForDialog();

        ById(dialog, "LastNameField").AsTextBox().Text = "Müller";
        ById(dialog, "FirstNameField").AsTextBox().Text = "Anna";
        ById(dialog, "PhoneNumberField").AsTextBox().Text = "0172 12345678";

        Assert.Contains("internationalen Format", ById(dialog, "PhoneNumberError").Name);
        Assert.True(Eventually(() => !ById(dialog, "SaveButton").IsEnabled));

        ById(dialog, "PhoneNumberField").AsTextBox().Text = "+4917212345678";

        Assert.True(Eventually(() => ById(dialog, "SaveButton").IsEnabled));
    }

    [Fact]
    public void DeletingEmployeeAsksForConfirmationFirst()
    {
        Seed(db => db.Employees.Add(new Employee { LastName = "Müller", FirstName = "Anna" }));
        OpenArea("MitarbeiterInnen");
        Assert.Single(Retry(() => RowTexts("EmployeesGrid")));

        ById(MainWindow, "DeleteButton").AsButton().Invoke();
        ById(WaitForDialog(), MessageBoxNo).AsButton().Invoke();
        WaitUntilDialogClosed();

        Assert.Single(RowTexts("EmployeesGrid"));

        ById(MainWindow, "DeleteButton").AsButton().Invoke();
        ById(WaitForDialog(), MessageBoxYes).AsButton().Invoke();
        WaitUntilDialogClosed();

        Assert.True(Eventually(() => RowTexts("EmployeesGrid").Count == 0));
    }

    private const string MessageBoxYes = "6";
    private const string MessageBoxNo = "7";
}
