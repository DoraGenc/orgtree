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

        if (!db.Departments.Any())
        {
            var management = new Department { Name = "Geschäftsführung", ColorHex = "#FF5555" };
            db.Departments.Add(management);
            db.SaveChanges();

            db.Departments.Add(new Department { Name = "IT", ParentDepartmentId = management.Id, ColorHex = "#55AAFF" });
            db.Departments.Add(new Department { Name = "Vertrieb", ParentDepartmentId = management.Id, ColorHex = "#7ED957" });
            db.SaveChanges();
        }

        if (!db.Employees.Any())
        {
            var management = db.Departments.First(d => d.Name == "Geschäftsführung");
            var it = db.Departments.First(d => d.Name == "IT");

            var anna = new Employee { FirstName = "Anna", LastName = "Müller", DepartmentId = management.Id, DepartmentHead = true };
            var ben = new Employee { FirstName = "Ben", LastName = "Schmidt", DepartmentId = it.Id, DepartmentHead = true };
            db.Employees.Add(anna);
            db.Employees.Add(ben);
            db.SaveChanges();

            management.LeaderId = anna.Id;
            it.LeaderId = ben.Id;
            db.SaveChanges();
        }
    }
}
