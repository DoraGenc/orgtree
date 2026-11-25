using Microsoft.EntityFrameworkCore;

namespace OrgTree.Core;

public class OrgTreeContext : DbContext
{
    public OrgTreeContext()
    {
    }

    public OrgTreeContext(DbContextOptions<OrgTreeContext> options)
        : base(options)
    {
    }

    public static string DatabasePath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OrgTree",
        "orgtree.db");

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    public void AssignDepartmentHead(Employee employee, Department? department)
    {
        var keptDepartmentId = department?.Id;

        foreach (var previouslyLed in Departments.Where(d => d.LeaderId == employee.Id && d.Id != keptDepartmentId))
        {
            previouslyLed.LeaderId = null;
        }

        if (department == null)
        {
            return;
        }

        department.LeaderId = employee.Id;
        employee.DepartmentId = department.Id;
    }

    public void RemoveDepartment(Department department)
    {
        foreach (var child in Departments.Where(d => d.ParentDepartmentId == department.Id))
        {
            child.ParentDepartmentId = null;
        }

        foreach (var member in Employees.Where(e => e.DepartmentId == department.Id))
        {
            member.DepartmentId = null;
        }

        Departments.Remove(department);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasOne<Department>()
            .WithMany()
            .HasForeignKey(employee => employee.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Department>()
            .HasOne<Employee>()
            .WithMany()
            .HasForeignKey(department => department.LeaderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Department>()
            .HasOne<Department>()
            .WithMany()
            .HasForeignKey(department => department.ParentDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Department>()
            .Property(department => department.Name)
            .UseCollation("NOCASE");

        modelBuilder.Entity<Department>()
            .HasIndex(department => department.Name)
            .IsUnique();
    }
}
