using Microsoft.EntityFrameworkCore;
using SDET_POC.API.Persistence.Entities;
using System.Reflection;

namespace SDET_POC.API.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    public DbSet<EmployeeEntity> Employees { get; set; }
    public DbSet<DepartmentEntity> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("sdet");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
