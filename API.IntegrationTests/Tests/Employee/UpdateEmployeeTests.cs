
using API.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using SDET_POC.API.Features.Employee.Commands;
using SDET_POC.API.Features.Models;
using SDET_POC.API.Persistence;
using SDET_POC.API.Persistence.Entities;
using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests.Tests.Employee;

public sealed class UpdateEmployeeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private Guid? existingEmployeeId;
    private Guid? existingDepartmentId;
    protected override async Task SeedDataAsync(ApplicationDbContext _context)
    {
        var employeeEntity = new EmployeeEntity
        {
            EmployeeName = "Sample Employee",
            Address = "sample address",
            CreatedByUserEmail = "test_email@metro.global"
        };
        _context.Employees.Add(employeeEntity);

        var departmentEntity = new DepartmentEntity
        {
            DepartmentName = "Temp Department",
            IsActive = true,
            CreatedByUserEmail = "test_email@metro.global"
        };
        _context.Departments.Add(departmentEntity);

        await _context.SaveChangesAsync();

        existingEmployeeId = employeeEntity.Id;
        existingDepartmentId = departmentEntity.Id;
    }

    [Fact]
    public async Task PutAsync_ShouldSuccessfullyUpdateExistingEmployee()
    {
        //Arrange
        var command = new UpdateEmployeeCommand(
            EmployeeId: existingEmployeeId ?? throw new Exception("Failed to set department head during test"),
            EmployeeName: "New sample address",
            Address: "New Address",
            IsActive: true,
            DepartmentId: null);

        //Act
        var response = await RegularUserClient.PutAsJsonAsync($"api/employee/{existingEmployeeId}", command);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedEmployee = await response.Content.ReadFromJsonAsync<EmployeeResponseDto?>();
        updatedEmployee.Should().NotBeNull();
        updatedEmployee.EmployeeName.Should().Be("New sample address");
        updatedEmployee.DepartmentName.Should().BeNull();
    }

    [Fact]
    public async Task PutAsync_ShouldSuccessfullyUpdateExistingEmployeeWithNewDepartment()
    {
        //Arrange
        var command = new UpdateEmployeeCommand(
            EmployeeId: existingEmployeeId ?? throw new Exception("Failed to set department head during test"),
            EmployeeName: "New sample address",
            Address: "New Address",
            IsActive: true,
            DepartmentId: existingDepartmentId);

        //Act
        var response = await RegularUserClient.PutAsJsonAsync($"api/employee/{existingEmployeeId}", command);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedEmployee = await response.Content.ReadFromJsonAsync<EmployeeResponseDto?>();
        updatedEmployee.Should().NotBeNull();
        updatedEmployee.EmployeeName.Should().Be("New sample address");
        updatedEmployee.DepartmentName.Should().Be("Temp Department");
    }
}