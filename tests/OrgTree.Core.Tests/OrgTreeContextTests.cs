using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrgTree.Core;

namespace OrgTree.Core.Tests;

public class DepartmentRelationshipMigrationTests
{
    [Fact]
    public void MigrationsEnforceDepartmentReferencesAndDeletionRules()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<OrgTreeContext>()
            .UseSqlite(connection)
            .Options;

        using var db = new OrgTreeContext(options);
        db.Database.Migrate("20261113063454_RemoveEmployeeDepartmentHead");

        db.Database.ExecuteSqlRaw(
            "INSERT INTO Departments (Name, LeaderId, ParentDepartmentId) VALUES ('Engineering', 999, 999)");
        var departmentId = db.Departments.Max(department => department.Id);
        db.Database.ExecuteSqlRaw(
            "INSERT INTO Employees (FirstName, LastName, DepartmentId) VALUES ('Ada', 'Lovelace', {0})",
            departmentId);
        db.Database.ExecuteSqlRaw(
            "INSERT INTO Employees (FirstName, LastName, DepartmentId) VALUES ('Grace', 'Hopper', NULL)");
        db.Database.ExecuteSqlRaw(
            "INSERT INTO Employees (FirstName, LastName, DepartmentId) VALUES ('Alan', 'Turing', 999)");

        db.Database.Migrate("20261113093528_AddUnassignedDepartment");

        var leader = new Employee
        {
            FirstName = "Marie",
            LastName = "Curie",
            DepartmentId = -1
        };
        var parent = new Department { Name = "Parent" };
        var child = new Department { Name = "Child" };
        var orphan = new Department { Name = "Orphan", LeaderId = 999, ParentDepartmentId = 999 };
        db.Employees.Add(leader);
        db.Departments.AddRange(parent, child, orphan);
        db.SaveChanges();

        db.Database.ExecuteSqlRaw(
            "UPDATE Departments SET LeaderId = {0} WHERE Id = {1}",
            leader.Id,
            parent.Id);
        db.Database.ExecuteSqlRaw(
            "UPDATE Departments SET ParentDepartmentId = {0} WHERE Id = {1}",
            parent.Id,
            child.Id);

        db.Database.Migrate();
        db.ChangeTracker.Clear();

        db.Employees.Add(new Employee { FirstName = "Katherine", LastName = "Johnson" });
        db.SaveChanges();
        db.ChangeTracker.Clear();

        Assert.Equal(
            departmentId,
            db.Employees.Single(employee => employee.FirstName == "Ada").DepartmentId);
        Assert.Null(db.Employees.Single(employee => employee.FirstName == "Grace").DepartmentId);
        Assert.Null(db.Employees.Single(employee => employee.FirstName == "Alan").DepartmentId);
        Assert.Null(db.Employees.Single(employee => employee.FirstName == "Katherine").DepartmentId);
        Assert.False(db.Departments.Any(department => department.Id == -1));
        Assert.Null(db.Departments.Single(department => department.Id == departmentId).LeaderId);
        Assert.Null(db.Departments.Single(department => department.Id == departmentId).ParentDepartmentId);
        Assert.Null(db.Departments.Single(department => department.Name == "Orphan").LeaderId);
        Assert.Null(db.Departments.Single(department => department.Name == "Orphan").ParentDepartmentId);

        db.Database.ExecuteSqlRaw("DELETE FROM Departments WHERE Id = {0}", departmentId);
        db.ChangeTracker.Clear();

        Assert.Null(db.Employees.Single(employee => employee.FirstName == "Ada").DepartmentId);
        Assert.Throws<SqliteException>(() => db.Database.ExecuteSqlRaw(
            "INSERT INTO Employees (FirstName, LastName, DepartmentId) VALUES ('Katherine', 'Johnson', 999)"));

        db.Database.ExecuteSqlRaw("DELETE FROM Employees WHERE Id = {0}", leader.Id);
        db.ChangeTracker.Clear();

        Assert.Null(db.Departments.Single(department => department.Id == parent.Id).LeaderId);

        db.Database.ExecuteSqlRaw("DELETE FROM Departments WHERE Id = {0}", parent.Id);
        db.ChangeTracker.Clear();

        Assert.Null(db.Departments.Single(department => department.Id == child.Id).ParentDepartmentId);
    }
}
