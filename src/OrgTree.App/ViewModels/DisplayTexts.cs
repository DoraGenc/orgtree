namespace OrgTree.App.ViewModels;

internal static class DisplayTexts
{
    public const string NoDepartment = "Keine Abteilung zugeordnet";

    public const string PleaseSelect = "auswählen...";

    public const string DefaultDepartmentColor = "#EEEEEE";

    public static string YesNo(bool value)
    {
        return value ? "Ja" : "Nein";
    }
}
