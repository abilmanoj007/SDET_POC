using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Persistence;

namespace SDET_POC.API.Features.Employee.Queries;

public class GetEmployeesByDeptIdController(IRequestHandler<GetEmployeesByDeptIdQuery, List<EmployeesByDeptIdResponseDto>> handler) : ControllerBase
{
    [HttpGet]
    [Route("api/employee/by-department/{departmentId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync([FromRoute] Guid departmentId, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetEmployeesByDeptIdQuery(departmentId), cancellationToken);
        return Ok(result);
    }
}

public sealed record GetEmployeesByDeptIdQuery(
    Guid DepartmentId) : IRequest<List<EmployeesByDeptIdResponseDto>>;

public sealed class EmployeesByDeptIdResponseDto
{
    public Guid EmployeeId { get; set; }
    public required string EmployeeName { get; set; }
    public required string Address { get; set; }
}

// add fluent validation

public sealed class GetEmployeesByDeptIdQueryHandler : IRequestHandler<GetEmployeesByDeptIdQuery, List<EmployeesByDeptIdResponseDto>>
{
    private readonly ApplicationDbContext _context;
    public GetEmployeesByDeptIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<EmployeesByDeptIdResponseDto>> Handle(GetEmployeesByDeptIdQuery request, CancellationToken cancellationToken)
    {
        return _context.Employees.AsNoTracking()
            .Where(x => x.DepartmentId == request.DepartmentId)
            .Select(x => new EmployeesByDeptIdResponseDto
            {
                EmployeeId = x.Id,
                EmployeeName = x.EmployeeName,
                Address = x.Address
            })
            .ToListAsync(cancellationToken);
    }
}