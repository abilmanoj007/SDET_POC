using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SDET_POC.API.Common.Exceptions;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Common.Models;
using SDET_POC.API.Persistence;
using SDET_POC.API.Persistence.Entities;
using SDET_POC.API.Services;

namespace SDET_POC.API.Features.Employee.Commands;

public sealed class AddEmployeeController(IRequestHandler<AddEmployeeCommand, Guid> handler) : ControllerBase
{
    [HttpPost]
    [Route("api/employee")]
    [Authorize]
    public async Task<IActionResult> PostAsync([FromBody] AddEmployeeCommand command, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(command, cancellationToken);
        return Ok(result);
    }
}

public sealed record AddEmployeeCommand(
    string EmployeeName,
    string Address,
    Guid? DepartmentId,
    bool IsActive) : IRequest<Guid>;

// add fluent validation

public sealed class AddEmployeeCommandHandler : IRequestHandler<AddEmployeeCommand, Guid>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly AppSettings _appSettings;

    public AddEmployeeCommandHandler(ICurrentUserService currentUserService, IOptions<AppSettings> options, ApplicationDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
        _appSettings = options.Value;
    }

    public async Task<Guid> Handle(AddEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId != _appSettings.UserId)
        {
            throw new ForbiddenAccessException();
        }

        var employeeEntity = new EmployeeEntity
        {
            EmployeeName = command.EmployeeName,
            Address = command.Address,
            CreatedByUserEmail = _currentUserService.UserEmail,
            DepartmentId = command.DepartmentId,
            IsActive = command.IsActive
        };

        await _context.Employees.AddAsync(employeeEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return employeeEntity.Id;
    }
}
