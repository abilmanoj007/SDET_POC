using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDET_POC.API.Persistence.Entities;

namespace SDET_POC.API.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(b => b.EmployeeName).IsRequired().HasMaxLength(255);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(255);
        builder.Property(b => b.CreatedByUserEmail).IsRequired().HasMaxLength(255);
        builder.Property(b => b.CreationDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.Department)
              .WithMany()
              .HasForeignKey(e => e.DepartmentId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
