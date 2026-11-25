using OrgTree.App.ViewModels;

namespace OrgTree.App.Services;

internal interface IDialogService
{
    bool Confirm(string title, string message);

    bool ShowEditor(DepartmentEditViewModel viewModel);

    bool ShowEditor(EmployeeEditViewModel viewModel);
}
