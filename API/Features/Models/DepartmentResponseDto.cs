namespace SDET_POC.API.Features.Models;

public sealed class DepartmentResponseDto
{
    public Guid Id {  get; set; }
    public required string DepartmentName { get; set; }
    public string? DepartmentHead { get; set; }
}
