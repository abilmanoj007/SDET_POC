using System.Security.Claims;

namespace API.IntegrationTests.Infrastructure.AuthHandler;

public class TestClaimsProvider
{
    public IEnumerable<Claim> Claims { get; private set; }
    public TestClaimsProvider()
    {
        Claims =
        [
            new Claim(ClaimTypes.NameIdentifier, "default-test-user-id"),
            new Claim(ClaimTypes.Email, "testuser@company.com"),
            new Claim(ClaimTypes.Role, "User")
        ];
    }

    public TestClaimsProvider(IEnumerable<Claim> claims)
    {
        Claims = claims;
    }
}
