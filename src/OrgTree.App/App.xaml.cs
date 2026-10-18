using System.Windows;
using Microsoft.EntityFrameworkCore;
using OrgTree.Core;

namespace OrgTree.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using var db = new OrgTreeContext();
        db.Database.Migrate();
    }
}
