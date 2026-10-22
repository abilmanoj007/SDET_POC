using Azure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using SDET_POC.API.Common.AuthorizationRequirements;
using SDET_POC.API.Common.Exceptions;
using SDET_POC.API.Common.Messaging;
using SDET_POC.API.Common.Models;
using SDET_POC.API.Common.PipelineBehaviours;
using SDET_POC.API.Persistence;
using SDET_POC.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using ValidationException = SDET_POC.API.Common.Exceptions.ValidationException;

var builder = WebApplication.CreateBuilder(args);

// Configuration
//if (!builder.Environment.IsDevelopment())
//{
//    builder.Configuration.AddAzureKeyVault(new Uri($"https://{builder.Configuration["VaultName"]}.vault.azure.net"), new DefaultAzureCredential());
//    builder.Services.AddApplicationInsightsTelemetry();
//}
builder.WebHost.UseSentry(options =>
{
    options.AddExceptionFilterForType<ValidationException>();
    options.AddExceptionFilterForType<ConflictException>();
    options.AddExceptionFilterForType<NotFoundException>();

    // no need to log forbidden access exceptions in sentry during integration test
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.AddExceptionFilterForType<ForbiddenAccessException>();
    }
});

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services
    .AddOptions<ConnectionStrings>()
    .BindConfiguration("ConnectionStrings")
    .ValidateDataAnnotations()
    .Validate(cs => cs.Validate(cs.DefaultConnection), "Cannot Connect With Default database using the connection string")
    .Validate(cs => cs.Validate(cs.ExternalDbConnection), "Cannot Connect With external database using the connection string")
    .ValidateOnStart();
}

builder.Services
    .AddOptions<AppSettings>()
    .BindConfiguration("AppSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();

//Database setup
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(
        connectionString,
        b =>
        {
            b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            b.MigrationsHistoryTable("__EFMigrationsHistory", "sdet");
        }
    );
});

//CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(b =>
        b.WithOrigins(builder.Configuration.GetSection("CORS:Origins").Get<string[]>() ?? throw new Exception("Missing CORS:Origins In Settings"))
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    )
);

//Auth
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAdB2C", options);

            options.TokenValidationParameters.NameClaimType = "name";
        }, options => builder.Configuration.Bind("AzureAdB2C", options));
builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new HasClaimsRequirement("SDET_POC_USER_HAS_ACCESS"))
        .Build())
    .AddPolicy("Admin", policy =>
        policy.RequireAuthenticatedUser()
            .AddRequirements(new HasClaimsRequirement("SDET_POC_USER_HAS_ACCESS", "SDET_POC_ADMIN_HAS_ACCESS")));

builder.Services.AddSingleton<IAuthorizationHandler, HasClaimsHandler>();
builder.Services.AddHttpContextAccessor();

//CQRS
builder.Services.Scan(scan => scan.FromAssembliesOf(typeof(Program))
.AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)), publicOnly: false)
.AsImplementedInterfaces().WithScopedLifetime()
.AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)), publicOnly: false)
.AsImplementedInterfaces().WithScopedLifetime()
);

builder.Services.Decorate(typeof(IRequestHandler<,>), typeof(UnhandledExceptionPipelineBehaviour.RequestHandler<,>));
builder.Services.Decorate(typeof(IRequestHandler<>), typeof(UnhandledExceptionPipelineBehaviour.RequestBaseHandler<>));

builder.Services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationPipelineBehaviour.RequestHandler<,>));
builder.Services.Decorate(typeof(IRequestHandler<>), typeof(ValidationPipelineBehaviour.RequestBaseHandler<>));

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

//API
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

//Cache
builder.Services.AddMemoryCache();
builder.Services.AddFusionCache();

//Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddHttpClient<ICW1Client, CW1Client>("eAdaptor", c =>
{
    c.BaseAddress = new Uri(builder.Configuration.GetSection("AppSettings:CW1Settings:BaseUri").Get<string>()!);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    var username = builder.Configuration.GetSection("AppSettings:CW1Settings:UserName").Get<string>();
    var password = builder.Configuration.GetSection("AppSettings:CW1Settings:Password").Get<string>();
    var byteArray = Encoding.GetEncoding("iso-8859-1").GetBytes($"{username}:{password}");
    var base64String = Convert.ToBase64String(byteArray);
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
});

//HC
builder.Services.AddHealthChecks();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//DB Migration
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await context.DisposeAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHealthChecks("/hc");
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();