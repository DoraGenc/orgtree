using OrgTree.App.Services;
using OrgTree.App.ViewModels;

namespace OrgTree.App.Tests;

internal sealed class StubDialogService : IDialogService
{
    public bool ConfirmResult { get; set; } = true;

    public List<string> ConfirmTitles { get; } = [];

    public bool Confirm(string title, string message)
    {
        ConfirmTitles.Add(title);
        return ConfirmResult;
    }

    public bool ShowEditor(DepartmentEditViewModel viewModel)
    {
        return false;
    }

    public bool ShowEditor(EmployeeEditViewModel viewModel)
    {
        return false;
    }
}
