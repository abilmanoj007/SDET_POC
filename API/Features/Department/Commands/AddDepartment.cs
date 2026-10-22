using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Persistence;
using SDET_POC.API.Persistence.Entities;
using SDET_POC.API.Services;

namespace SDET_POC.API.Features.Department.Commands;

public sealed class AddDepartmentController(IRequestHandler<AddDepartmentCommand> handler) : ControllerBase
{
    [HttpPost]
    [Route("api/department")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> PostAsync([FromBody] AddDepartmentCommand command, CancellationToken cancellationToken)
    {
        await handler.Handle(command, cancellationToken);
        return Ok();
    }
}
public sealed record AddDepartmentCommand(
    string DepartmentName,
    Guid DepartmentHeadId,
    bool IsActive
    ) : IRequest;

// Add fluent validation

public sealed class AddDepartmentCommandHandler : IRequestHandler<AddDepartmentCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddDepartmentCommandHandler(ICurrentUserService currentUserService, ApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task Handle(AddDepartmentCommand command, CancellationToken cancellationToken)
    {
        var departmentEntity = new DepartmentEntity
        {
            DepartmentName = command.DepartmentName,
            CreatedByUserEmail = _currentUserService.UserEmail,
            DepartmentHeadId = command.DepartmentHeadId,
            IsActive = command.IsActive
        };

        await _context.Departments.AddAsync(departmentEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
