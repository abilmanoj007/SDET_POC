using API.IntegrationTests.Infrastructure.AuthHandler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Respawn;
using Dapper;
using SDET_POC.API.Common.Models;
using SDET_POC.API.Persistence;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Reflection;
using Testcontainers.MsSql;

namespace API.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _appDbContainer 
        = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

    private readonly MsSqlContainer _externalDbContainer
        = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

    private Respawner _appDbRespawner = null!;
    private DbConnection _appDbConnection = null!;
    private Respawner _externalDbRespawner = null!;
    private DbConnection _externalDbConnection = null!;

    public string ExternalDbConnectionString =>
        _externalDbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            // replacing real db instance with test container db
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_appDbContainer.GetConnectionString());
            });

            // replace external db connection string with test containers
            services.PostConfigure<ConnectionStrings>(options =>
            {
                options.ExternalDbConnection = _externalDbContainer.GetConnectionString();
            });

            // auth replacement
            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();
            services.AddSingleton(new TestClaimsProvider());

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, options => { });

            services.AddSingleton<TestUsers>();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _appDbRespawner.ResetAsync(_appDbConnection);
        await _externalDbRespawner.ResetAsync(_externalDbConnection);
    }

    public HttpClient CreateClientAs(TestUserType userType)
    {
        var testUsers = Services.GetRequiredService<TestUsers>();
        var claims = testUsers.GetClaims(userType);

        var client = WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TestClaimsProvider>();
                services.AddSingleton(new TestClaimsProvider(claims));
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestAuthHandler.TestToken);

        return client;
    }

    public async Task InitializeAsync()
    {
        // start db containers
        await Task.WhenAll(
            _appDbContainer.StartAsync(),
            _externalDbContainer.StartAsync()
            );

        // apply db migrations or setup tables/stored procedures
        await Task.WhenAll(
            ApplyDBMigrationsAsync(),
            SetupExternalDatabaseAsync()
            );

        // setup respawner to clear databases after test runs
        await Task.WhenAll(
            SetupAppDbRespawnerAsync(),
            SetupExternalDbRespawnerAsync()
            );
    }

    private async Task ApplyDBMigrationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private async Task SetupExternalDatabaseAsync()
    {
        using var connection = new SqlConnection(
            _externalDbContainer.GetConnectionString());
        await connection.OpenAsync();

        await ExecuteSqlFilesFromFolder(connection, "Tables");
        await ExecuteSqlFilesFromFolder(connection, "StoredProcedures");
    }
    private async Task SetupAppDbRespawnerAsync()
    {
        _appDbConnection = new SqlConnection(_appDbContainer.GetConnectionString());
        await _appDbConnection.OpenAsync();

        _appDbRespawner = await Respawner.CreateAsync(_appDbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude =
            [
                "sdet"
            ]
        });
    }
    private async Task SetupExternalDbRespawnerAsync()
    {
        _externalDbConnection = new SqlConnection(_externalDbContainer.GetConnectionString());
        await _externalDbConnection.OpenAsync();

        _externalDbRespawner = await Respawner.CreateAsync(_externalDbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude =
            [
                "dbo"
            ]
        });
    }
    private static async Task ExecuteSqlFilesFromFolder(SqlConnection connection, string folder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePrefix =
            $"API.IntegrationTests.Infrastructure.ExternalDatabase.{folder}";

        var sqlResources = assembly
            .GetManifestResourceNames()
            .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".sql"))
            .OrderBy(r => r);   // numeric prefixes must be in the order files have to be executed

        foreach( var resource in sqlResources)
        {
            using var stream = assembly.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync();
            await connection.ExecuteAsync(sql);
        }

    }
    public new async Task DisposeAsync()
    {
        await Task.WhenAll(
            _appDbConnection.DisposeAsync().AsTask(),
            _externalDbConnection.DisposeAsync().AsTask());
        await Task.WhenAll(
            _appDbContainer.StopAsync(),
            _externalDbContainer.StopAsync());
        await base.DisposeAsync();
    }
}
