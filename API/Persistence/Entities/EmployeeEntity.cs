namespace SDET_POC.API.Persistence.Entities;

public sealed class EmployeeEntity
{
    public Guid Id { get; set; }
    public required string EmployeeName { get; set; }
    public required string Address { get; set; }
    public DateTime CreationDate { get; set; }
    public required string CreatedByUserEmail { get; set; }
    public bool IsActive { get; set; }
    public Guid? DepartmentId { get; set; }
    public DepartmentEntity? Department {  get; set; }
}
