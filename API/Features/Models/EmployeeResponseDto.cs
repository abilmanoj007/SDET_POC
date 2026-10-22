
namespace SDET_POC.API.Features.Models;

public sealed class EmployeeResponseDto
{
    public Guid Id { get; set; }
    public required string EmployeeName { get; set; }

    public string? DepartmentName { get; set; }
}
