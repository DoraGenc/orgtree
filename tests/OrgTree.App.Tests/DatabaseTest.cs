using System.IO;
using Microsoft.EntityFrameworkCore;
using OrgTree.Core;

namespace OrgTree.App.Tests;

[Collection(DatabaseCollection.Name)]
public abstract class DatabaseTest
{
    protected DatabaseTest()
    {
        if (!Path.GetFileName(OrgTreeContext.DatabasePath).StartsWith(DatabaseFixture.TestFilePrefix))
        {
            throw new InvalidOperationException(
                $"Test würde die echte Datenbank benutzen: {OrgTreeContext.DatabasePath}");
        }

        using var db = new OrgTreeContext();
        db.Database.ExecuteSqlRaw("DELETE FROM Employees");
        db.Database.ExecuteSqlRaw("DELETE FROM Departments");
        db.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence");
    }
}
