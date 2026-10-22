using API.IntegrationTests.Infrastructure;
using FluentAssertions;
using Qase.Csharp.Commons.Attributes;
using SDET_POC.API.Features.Employee.Commands;
using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests.Tests.Employee;

public sealed class AddEmployeeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    [Title("Get status code 200")]
    public async Task PostAsync_ShouldReturn200_WhenCommandIsValid()
    {
        //Arrange
        var command = new AddEmployeeCommand("Tim Horton", "12 Beverly Hills, Birmingham, B34 FTY", null, true);

        //Act
        var response = await RegularUserClient.PostAsJsonAsync("api/employee", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeeId = await response.Content.ReadFromJsonAsync<Guid>();
        employeeId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task PostAsync_ShouldReturn200_WhenAdminDoesInsert()
    {
        //Arrange
        var command = new AddEmployeeCommand("Tim Horton", "12 Beverly Hills, Birmingham, B34 FTY", null, true);

        //Act
        var response = await AdminClient.PostAsJsonAsync("api/employee", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employeeId = await response.Content.ReadFromJsonAsync<Guid>();
        employeeId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task PostAsync_ShouldReturn403_WhenForbiddenUserTriesInsert()
    {
        // Arrange
        var command = new AddEmployeeCommand("Tim Horton", "12 Beverly Hills, Birmingham, B34 FTY", null, true);

        // Act
        var response = await ForbiddenUserClient.PostAsJsonAsync("api/employee", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
