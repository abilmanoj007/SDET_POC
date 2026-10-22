using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDET_POC.API.Common.Exceptions;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Features.Models;
using SDET_POC.API.Persistence;

namespace SDET_POC.API.Features.Employee.Commands;

public class UpdateEmployeeController(IRequestHandler<UpdateEmployeeCommand, EmployeeResponseDto?> handler) : ControllerBase
{
    [HttpPut]
    [Route("api/employee/{employeeId}")]
    [Authorize]
    public async Task<IActionResult> PutAsync([FromRoute] Guid employeeId, [FromBody] UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if(employeeId != command.EmployeeId)
        {
            BadRequest("EmployeeId in request body and path do not match");
        }
        var result = await handler.Handle(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateEmployeeCommand(
    Guid EmployeeId,
    string EmployeeName,
    string Address,
    bool IsActive,
    Guid? DepartmentId) : IRequest<EmployeeResponseDto?>;

// add fluent validation

public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeResponseDto?>
{
    private readonly ApplicationDbContext _context;

    public UpdateEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<EmployeeResponseDto?> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employeeById = await _context.Employees.FirstOrDefaultAsync(x => x.Id == command.EmployeeId, cancellationToken)
                        ?? throw new NotFoundException($"Cannot find employee details with id {command.EmployeeId}");

        employeeById.EmployeeName = command.EmployeeName;
        employeeById.Address = command.Address;
        employeeById.IsActive = command.IsActive;
        employeeById.DepartmentId = command.DepartmentId;

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Employees.AsNoTracking().Where(x => x.Id == command.EmployeeId)
            .Select(x => new EmployeeResponseDto
            {
                Id = x.Id,
                EmployeeName = x.EmployeeName,
                DepartmentName = x.Department.DepartmentName
            })
            .FirstOrDefaultAsync(cancellationToken);  
    }
}