
using API.IntegrationTests.Infrastructure;
using FluentAssertions;
using SDET_POC.API.Features.Projects.Commands;
using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests.Tests.Project;

public class AddProjectTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private static readonly Guid SeededProjectId =
        Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    [Fact]
    public async Task CreateProject_ShouldReturn200_WhenRequestIsValid()
    {
        //Arrange
        var command = new AddProjectCommand(
            Id: SeededProjectId,
            ProjectName: "Eco-Pledge data app",
            DepartmentName: "Technical Solutions");

        //Act
        var response = await RegularUserClient.PostAsJsonAsync("api/project", command);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectId = await response.Content.ReadFromJsonAsync<Guid>();
        projectId.Should().Be(SeededProjectId);
    }
}
