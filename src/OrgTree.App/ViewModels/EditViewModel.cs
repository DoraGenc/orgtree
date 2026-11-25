using CommunityToolkit.Mvvm.ComponentModel;

namespace OrgTree.App.ViewModels;

internal abstract class EditViewModel : ObservableObject
{
    protected EditViewModel(int? id)
    {
        Id = id;
    }

    public event EventHandler<bool>? CloseRequested;

    public int? Id { get; }

    public bool HasId => Id != null;

    public string Heading => HasId ? "Editieren" : "Erstellen";

    public string DisplayName { get; protected set; } = "";

    protected void RequestClose(bool saved)
    {
        CloseRequested?.Invoke(this, saved);
    }
}
