using Microsoft.AspNetCore.Authorization;

namespace SDET_POC.API.Common.AuthorizationRequirements;

public sealed class HasClaimsRequirement(string role, params string[] roles) : IAuthorizationRequirement
{
    public string[] RequiredRoles { get; } = [role, .. roles];
}

public sealed class HasClaimsHandler : AuthorizationHandler<HasClaimsRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasClaimsRequirement requirement)
    {
        var claims = context.User.Claims.ToList();
        if (string.IsNullOrWhiteSpace(claims.FirstOrDefault(c => c.Type == "mail")?.Value))
        {
            context.Fail();
            return Task.FromResult(0);
        }

        var rolesString = claims.FirstOrDefault(c => c.Type == "roles")?.Value;
        if (string.IsNullOrWhiteSpace(rolesString) || !requirement.RequiredRoles.All(rolesString.Contains))
        {
            context.Fail();
            return Task.FromResult(0);
        }

        if (!Guid.TryParse(claims.FirstOrDefault(c => c.Type == "sub")?.Value, out Guid sub))
        {
            context.Fail();
            return Task.FromResult(0);
        }

        context.Succeed(requirement);
        return Task.FromResult(0);
    }
}
