using System.Diagnostics;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.Data.Sqlite;

namespace OrgTree.App.UiTests;

[Collection("UI")]
public abstract class AppTest : IDisposable
{
    private readonly string _databasePath;

    protected Application App { get; }
    protected UIA3Automation Automation { get; }
    protected Window MainWindow { get; }

    protected AppTest()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"orgtree-uitest-{Guid.NewGuid()}.db");

        var startInfo = new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "OrgTree.App.exe"));
        startInfo.Environment["ORGTREE_DATABASE_PATH"] = _databasePath;

        App = Application.Launch(startInfo);
        Automation = new UIA3Automation();
        MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(15))
            ?? throw new InvalidOperationException("Hauptfenster ist nicht erschienen.");
    }

    public void Dispose()
    {
        App.Close();
        if (!App.HasExited)
        {
            App.Kill();
        }

        App.Dispose();
        Automation.Dispose();
        SqliteConnection.ClearAllPools();
        DeleteWithRetry(_databasePath);
        GC.SuppressFinalize(this);
    }

    protected static List<T> Retry<T>(Func<List<T>> find)
    {
        var result = FlaUI.Core.Tools.Retry.WhileEmpty(find, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(100));
        return result.Result ?? [];
    }

    private static void DeleteWithRetry(string path)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(200);
            }
        }
    }
}
