
using API.IntegrationTests.Infrastructure;
using FluentAssertions;
using SDET_POC.API.Features.Models;
using SDET_POC.API.Persistence;
using SDET_POC.API.Persistence.Entities;
using System.Net;
using System.Net.Http.Json;

namespace API.IntegrationTests.Tests.Department;

public sealed class GetDepartmentByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private Guid? existingDepartmentId;
    protected override async Task SeedDataAsync(ApplicationDbContext _context)
    {
        var departmentEntity = new DepartmentEntity
        {
            DepartmentName = "Temp Department",
            IsActive = true,
            CreatedByUserEmail = "test_email@metro.global"
        };
        _context.Departments.Add(departmentEntity);

        await _context.SaveChangesAsync();

        existingDepartmentId = departmentEntity.Id;
    }

    [Fact]
    public async Task GetAsync_ShouldSuccessfullyGetDepartmentByIdDetails()
    {
        //Arrange

        //Act
        var response = await RegularUserClient.GetAsync($"api/department/{existingDepartmentId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var department = await response.Content.ReadFromJsonAsync<DepartmentResponseDto?>();
        department.Should().NotBeNull();
        department.DepartmentName.Should().Be("Temp Department");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNullWhenNoMatchingDepartmentFound()
    {
        //Arrange
        var randomId = Guid.NewGuid();

        //Act
        var response = await RegularUserClient.GetAsync($"api/department/{randomId}");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
