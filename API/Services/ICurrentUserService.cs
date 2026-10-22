using SDET_POC.API.Common.Exceptions;

namespace SDET_POC.API.Services;

public interface ICurrentUserService
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string UserEmail { get; }
}
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public Guid UserId => Guid.Parse(_httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? throw new ForbiddenAccessException());

    public string UserName => _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value ?? throw new ForbiddenAccessException();

    public string UserEmail => _httpContextAccessor.HttpContext?.User?.FindFirst("mail")?.Value ?? throw new ForbiddenAccessException();
}
