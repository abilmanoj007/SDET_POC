using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Common.Models;
using System.Data;

namespace SDET_POC.API.Features.Projects.Queries;

public sealed class GetProjectByIdController(IRequestHandler<GetProjectByIdQuery, ProjectByIdResponseDto> handler) : ControllerBase
{
    [HttpGet]
    [Route("api/project/{projectId}")]
    [Authorize]
    public async Task<IActionResult> GetAsync([FromRoute] Guid projectId, CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetProjectByIdQuery(projectId), cancellationToken);
        return Ok(result);
    }
}


public sealed record GetProjectByIdQuery(
    Guid ProjectId) : IRequest<ProjectByIdResponseDto>;

public sealed class ProjectByIdResponseDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectByIdResponseDto>
{
    private readonly ConnectionStrings _connectionStrings;
    public GetProjectByIdQueryHandler(IOptions<ConnectionStrings> options)
    {
        _connectionStrings = options.Value;
    }
    public async Task<ProjectByIdResponseDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = new SqlConnection(_connectionStrings.ExternalDbConnection);
        var command = new CommandDefinition("dbo.GetProjectById", new { Id = request.ProjectId}, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return await connection.QueryFirstAsync<ProjectByIdResponseDto>(command);
    }
}