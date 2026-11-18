using System.Diagnostics;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrgTree.Core;

namespace OrgTree.App.UiTests;

[Collection("UI")]
public abstract class AppTest : IDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(100);

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
        foreach (var dialog in MainWindow.ModalWindows)
        {
            dialog.Close();
        }

        App.Close();
        if (!App.HasExited)
        {
            App.Kill();
        }

        App.Dispose();
        Automation.Dispose();
        SqliteConnection.ClearAllPools();
        DeleteWithRetry(_databasePath);
        DeleteWithRetry(_databasePath + "-wal");
        DeleteWithRetry(_databasePath + "-shm");
        GC.SuppressFinalize(this);
    }

    protected void Seed(Action<OrgTreeContext> seed)
    {
        var options = new DbContextOptionsBuilder<OrgTreeContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        using var db = new OrgTreeContext(options);
        seed(db);
        db.SaveChanges();
    }

    protected void OpenArea(string name)
    {
        WaitFor(
                () => MainWindow.FindFirstDescendant(c => c.ByControlType(ControlType.Button).And(c.ByName(name))),
                $"Button '{name}' auf der Startseite")
            .AsButton()
            .Invoke();
    }

    protected static AutomationElement ById(AutomationElement parent, string automationId)
    {
        return WaitFor(
            () => parent.FindFirstDescendant(c => c.ByAutomationId(automationId)),
            $"Element '{automationId}'");
    }

    protected Window WaitForDialog()
    {
        return WaitFor(() => MainWindow.ModalWindows.FirstOrDefault(), "Dialogfenster").AsWindow();
    }

    protected void WaitUntilDialogClosed()
    {
        Assert.True(Eventually(() => MainWindow.ModalWindows.Length == 0), "Dialogfenster hat sich nicht geschlossen.");
    }

    protected List<string> RowTexts(string gridAutomationId)
    {
        var grid = ById(MainWindow, gridAutomationId).AsGrid();
        return grid.Rows
            .Select(row => string.Join(" | ", row
                .FindAllDescendants(c => c.ByControlType(ControlType.Text))
                .Select(text => text.Name)))
            .ToList();
    }

    protected static List<T> Retry<T>(Func<List<T>> find)
    {
        return FlaUI.Core.Tools.Retry.WhileEmpty(find, Timeout, Interval).Result ?? [];
    }

    protected static bool Eventually(Func<bool> condition)
    {
        return FlaUI.Core.Tools.Retry.WhileFalse(condition, Timeout, Interval).Result;
    }

    private static AutomationElement WaitFor(Func<AutomationElement?> find, string description)
    {
        return FlaUI.Core.Tools.Retry.WhileNull(find, Timeout, Interval).Result
            ?? throw new InvalidOperationException($"{description} nicht gefunden.");
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
