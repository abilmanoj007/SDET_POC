using API.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SDET_POC.API.Features.Department.Commands;
using SDET_POC.API.Persistence;
using SDET_POC.API.Persistence.Entities;
using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests.Tests.Department;

public sealed class AddDepartmentTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private Guid? departmentHeadId;

    protected override async Task SeedDataAsync(ApplicationDbContext _context)
    {
        var employeeEntity = new EmployeeEntity
        {
            EmployeeName = "Sample Manager",
            Address = "sample address",
            CreatedByUserEmail = "test_email@metro.global"
        };
        _context.Employees.Add(employeeEntity);
        await _context.SaveChangesAsync();

        departmentHeadId = employeeEntity.Id;
    }

    [Fact]
    public async Task CreateDepartment_ShouldReturn200_WhenRequestIsValid()
    {
        //Arrange
        var command = new AddDepartmentCommand(
            DepartmentName: "Technical Solutions",
            DepartmentHeadId: departmentHeadId ?? throw new Exception("Failed to set department head during test"),
            IsActive: true);

        //Act
        var response = await AdminClient.PostAsJsonAsync("api/department", command);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostAsync_ShouldReturn403_WhenUserLacksPermission()
    {
        //Arrange
        var command = new AddDepartmentCommand(
            DepartmentName: "Technical Solutions",
            DepartmentHeadId: departmentHeadId ?? throw new Exception("Failed to set department head during test"),
            IsActive: true);

        //Act
        var response = await RegularUserClient.PostAsJsonAsync("api/department", command);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostAsync_ShouldReturn401_WhenNoTokenFound()
    {
        //Arrange
        var command = new AddDepartmentCommand(
            DepartmentName: "Technical Solutions",
            DepartmentHeadId: departmentHeadId ?? throw new Exception("Failed to set department head during test"),
            IsActive: true);

        var client = _factory.CreateClient();

        //Act
        var response = await client.PostAsJsonAsync("api/department", command);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}
