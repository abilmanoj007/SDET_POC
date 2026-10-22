using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SDET_POC.API.Common.Exceptions;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Features.Models;
using SDET_POC.API.Persistence;

namespace SDET_POC.API.Features.Department.Commands;

public sealed class UpdateDepartmentController(IRequestHandler<UpdateDepartmentCommand, DepartmentResponseDto?> handler) : ControllerBase
{
    [HttpPut]
    [Route("api/department/{departmentId}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> PutAsync([FromRoute] Guid departmentId, [FromBody] UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        if(departmentId != command.DepartmentId)
        {
            BadRequest("DepartmentId in request body and path do not match");
        }
        await handler.Handle(command, cancellationToken);
        return Ok();
    }
}

public sealed record UpdateDepartmentCommand(
    Guid DepartmentId,
    string DepartmentName,
    Guid DepartmentHeadId,
    bool IsActive) : IRequest<DepartmentResponseDto?>;


// add fluent validation

public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentResponseDto?>
{
    private readonly ApplicationDbContext _context;
    public UpdateDepartmentCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepartmentResponseDto?> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        var departmentById = await _context.Departments.FirstOrDefaultAsync(x => x.Id == command.DepartmentId, cancellationToken)
                                ?? throw new NotFoundException($"Cannot find department with id {command.DepartmentId}");

        departmentById.DepartmentName = command.DepartmentName;
        departmentById.DepartmentHeadId = command.DepartmentHeadId;
        departmentById.IsActive = command.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Departments
            .Where(x => x.Id == command.DepartmentId)
            .Select(x => new DepartmentResponseDto
            {
                Id = x.Id,
                DepartmentName = x.DepartmentName,
                DepartmentHead = x.DepartmentHead.EmployeeName
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}