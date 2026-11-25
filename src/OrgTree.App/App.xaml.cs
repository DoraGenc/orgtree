using System.Windows;
using Microsoft.EntityFrameworkCore;
using OrgTree.Core;

namespace OrgTree.App;

internal sealed partial class App : Application
{
    private const string DatabasePathVariable = "ORGTREE_DATABASE_PATH";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var databasePath = Environment.GetEnvironmentVariable(DatabasePathVariable);
        if (!string.IsNullOrWhiteSpace(databasePath))
        {
            OrgTreeContext.DatabasePath = databasePath;
        }

        using var db = new OrgTreeContext();
        db.Database.Migrate();
    }
}
