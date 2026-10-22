using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Features.Models;
using SDET_POC.API.Persistence;

namespace SDET_POC.API.Features.Department.Queries;

public sealed class GetDepartmentByIdController(IRequestHandler<GetDepartmentByIdQuery, DepartmentResponseDto?> handler) : ControllerBase
{
    [HttpGet]
    [Route("api/department/{departmentId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync([FromRoute] Guid departmentId, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetDepartmentByIdQuery(departmentId), cancellationToken);
        return Ok(result);
    }
}

public sealed record GetDepartmentByIdQuery(
    Guid DepartmentId) : IRequest<DepartmentResponseDto?>;

public sealed class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentResponseDto?>
{
    private readonly ApplicationDbContext _context;
    public GetDepartmentByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<DepartmentResponseDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Departments
            .Where(x => x.Id == request.DepartmentId)
            .Select(x => new DepartmentResponseDto
            {
                Id = x.Id,
                DepartmentName = x.DepartmentName,
                DepartmentHead = x.DepartmentHead.EmployeeName
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}