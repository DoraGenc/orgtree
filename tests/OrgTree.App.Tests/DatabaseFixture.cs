using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrgTree.Core;

namespace OrgTree.App.Tests;

public class DatabaseFixture : IDisposable
{
    public const string TestFilePrefix = "orgtree-test-";

    private readonly string _originalPath;
    private readonly string _testPath;

    public DatabaseFixture()
    {
        _originalPath = OrgTreeContext.DatabasePath;
        _testPath = Path.Combine(Path.GetTempPath(), $"{TestFilePrefix}{Guid.NewGuid()}.db");
        OrgTreeContext.DatabasePath = _testPath;

        using var db = new OrgTreeContext();
        db.Database.Migrate();
    }

    public void Dispose()
    {
        OrgTreeContext.DatabasePath = _originalPath;
        SqliteConnection.ClearAllPools();
        File.Delete(_testPath);
        GC.SuppressFinalize(this);
    }
}

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    public const string Name = "Database";
}
