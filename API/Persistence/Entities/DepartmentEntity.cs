namespace SDET_POC.API.Persistence.Entities;

public sealed class DepartmentEntity
{
    public Guid Id { get; set; }
    public required string DepartmentName { get; set; }
    public DateTime CreationDate {  get; set; }
    public required string CreatedByUserEmail { get; set; }
    public Guid? DepartmentHeadId { get; set; }
    public EmployeeEntity? DepartmentHead {  get; set; }
    public bool IsActive { get; set; }
}
