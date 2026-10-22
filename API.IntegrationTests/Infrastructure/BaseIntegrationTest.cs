using API.IntegrationTests.Infrastructure.AuthHandler;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SDET_POC.API.Persistence;

namespace API.IntegrationTests.Infrastructure;

[Collection("SdetTestCollection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory _factory;
    private IServiceScope _scope = null!;

    protected HttpClient RegularUserClient { get; set; }
    protected HttpClient AdminClient { get; set; }
    protected HttpClient ForbiddenUserClient { get; set; }

    protected ApplicationDbContext _context { get; set; } = null!;

    protected SqlConnection _externalDbConnection { get; set; } = null!;

    public BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;

        RegularUserClient = factory.CreateClientAs(TestUserType.REGULAR);
        AdminClient = factory.CreateClientAs(TestUserType.ADMIN);
        ForbiddenUserClient = factory.CreateClientAs(TestUserType.FORBIDDEN);
    }

    public virtual async Task InitializeAsync()
    {
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        _externalDbConnection = new SqlConnection(_factory.ExternalDbConnectionString);
        await _externalDbConnection.OpenAsync();

        await SeedDataAsync(_context);
        await SeedDataAsync(_externalDbConnection);
        await SeedDataAsync(_context, _externalDbConnection);
    }

    protected virtual Task SeedDataAsync(
        ApplicationDbContext context) => Task.CompletedTask;

    protected virtual Task SeedDataAsync(
        SqlConnection externalDb) => Task.CompletedTask;

    protected virtual Task SeedDataAsync(
        ApplicationDbContext context,
        SqlConnection externalDb) => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _externalDbConnection.DisposeAsync();
        _scope.Dispose();
    }
}
