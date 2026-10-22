using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Common.Models;
using System.Data;

namespace SDET_POC.API.Features.Projects.Commands;

public sealed class AddProjectController(IRequestHandler<AddProjectCommand, Guid> handler) : ControllerBase
{
    [HttpPost]
    [Route("api/project")]
    [Authorize]
    public async Task<IActionResult> PostAsync([FromBody] AddProjectCommand command, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(command, cancellationToken);
        return Ok(result);
    }
}
public sealed record AddProjectCommand(
    Guid Id,
    string ProjectName,
    string DepartmentName) : IRequest<Guid>;

public sealed class AddProjectCommandHandler : IRequestHandler<AddProjectCommand, Guid>
{
    private readonly ConnectionStrings _connectionStrings;
    public AddProjectCommandHandler(IOptions<ConnectionStrings> options)
    {
        _connectionStrings = options.Value;
    }
    public async Task<Guid> Handle(AddProjectCommand request, CancellationToken cancellationToken)
    {
        using var connection = new SqlConnection(_connectionStrings.ExternalDbConnection);
        var sql =
            """
                INSERT INTO [dbo].[Projects](Id, Name, DepartmentName)
                VALUES (@Id, @ProjectName, @DepartmentName)
            """;
        var command = new CommandDefinition(sql, request, commandType: CommandType.Text, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
        return request.Id;
    }
}