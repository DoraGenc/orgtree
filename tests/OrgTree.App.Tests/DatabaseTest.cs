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

    protected static int AddDepartment(string name, int? parentDepartmentId = null, string? colorHex = null)
    {
        using var db = new OrgTreeContext();
        var department = new Department
        {
            Name = name,
            ParentDepartmentId = parentDepartmentId,
            ColorHex = colorHex
        };
        db.Departments.Add(department);
        db.SaveChanges();
        return department.Id;
    }

    protected static int AddEmployee(
        string lastName,
        string firstName,
        string? phoneNumber = null,
        int? departmentId = null,
        bool isLeader = false)
    {
        using var db = new OrgTreeContext();
        var employee = new Employee
        {
            LastName = lastName,
            FirstName = firstName,
            PhoneNumber = phoneNumber,
            DepartmentId = departmentId
        };
        db.Employees.Add(employee);
        db.SaveChanges();

        if (isLeader && departmentId != null)
        {
            db.Departments.Find(departmentId)!.LeaderId = employee.Id;
            db.SaveChanges();
        }

        return employee.Id;
    }
}
