using API.IntegrationTests.Infrastructure;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using SDET_POC.API.Features.Projects.Queries;
using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests.Tests.Project;

public sealed class GetProjectByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly Guid seededProjectId = Guid.NewGuid();
    protected override async Task SeedDataAsync(SqlConnection externalDb)
    {
        await externalDb.ExecuteAsync(@"
            INSERT INTO dbo.Projects (Id, Name, DepartmentName)
            VALUES (@Id, @Name, @DepartmentName)",
            new
            {
                Id = seededProjectId,
                Name = "Haulier App",
                DepartmentName = "Technical Solutions"
            });
    }

    [Fact]
    public async Task GetProjectById_ShouldReturnData_WhenRequestIsValid()
    {
        //Arrange

        //Act
        var response = await RegularUserClient.GetAsync($"api/project/{seededProjectId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var project = await response.Content.ReadFromJsonAsync<ProjectByIdResponseDto>();
        project.Should().NotBeNull();
        project.Id.Should().Be(seededProjectId);
        project.Name.Should().Be("Haulier App");
        project.DepartmentName.Should().Be("Technical Solutions");
    }
}
