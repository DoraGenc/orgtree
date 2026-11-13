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

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();

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
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OrgTree");
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, "orgtree.db");
        optionsBuilder.UseSqlite($"Data Source={file}");
    }
}
