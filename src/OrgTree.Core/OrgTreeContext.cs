using Microsoft.EntityFrameworkCore;

namespace OrgTree.Core;

public class OrgTreeContext : DbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OrgTree");
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, "orgtree.db");
        optionsBuilder.UseSqlite($"Data Source={file}");
    }
}
