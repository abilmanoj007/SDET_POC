using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SDET_POC.API.Persistence.Entities;

namespace SDET_POC.API.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<DepartmentEntity>
{
    public void Configure(EntityTypeBuilder<DepartmentEntity> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(b => b.DepartmentName).IsRequired().HasMaxLength(255);

        builder.Property(b => b.CreatedByUserEmail).IsRequired().HasMaxLength(255);
        builder.Property(b => b.CreationDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(d => d.DepartmentHead)
            .WithOne()
            .HasForeignKey<DepartmentEntity>(d => d.DepartmentHeadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
