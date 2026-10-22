using Microsoft.Extensions.Options;
using SDET_POC.API.Common.Models;
using System.Security.Claims;

namespace API.IntegrationTests.Infrastructure.AuthHandler;

public sealed class TestUsers
{
    private readonly AppSettings _appSettings;

    public TestUsers(IOptions<AppSettings> options)
    {
        _appSettings = options.Value;
    }
    public IEnumerable<Claim> GetClaims(TestUserType userType) =>
        userType switch
        {
            TestUserType.REGULAR => GetRegularUserClaims(),
            TestUserType.ADMIN => GetAdminUserClaims(),
            TestUserType.FORBIDDEN => GetForbiddenUserClaims(),
            _ => throw new InvalidOperationException()
        };

    // Regular User
    public  Claim[] GetRegularUserClaims() =>
    [
        new("mail", "test-regularuser@random_company.com"),
        new("roles", "SDET_POC_USER_HAS_ACCESS"),
        new("sub", _appSettings.UserId.ToString())
    ];

    // Admin User
    public Claim[] GetAdminUserClaims() =>
    [
        new("mail", "test-adminuser@metro.global"),
        new("roles", "SDET_POC_USER_HAS_ACCESS;SDET_POC_ADMIN_HAS_ACCESS"),
        new("sub", _appSettings.UserId.ToString())
    ];

    // Forbidden
    public Claim[] GetForbiddenUserClaims() =>
    [
        new("mail", "test-regularuser@random_company.com"),
        new("roles", "SDET_POC_USER_HAS_ACCESS"),
        new("sub", Guid.NewGuid().ToString())
    ];
}