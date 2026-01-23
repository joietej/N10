using System.Threading.RateLimiting;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using N10.WebApi.Apis;
using N10.WebApi.Extensions;
using N10.Data.Context;
using N10.Data.Init;
using N10.WebApi.GraphQL.Queries;
using Polly;
using Polly.Retry;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureOpenTelemetry();
// Add Http logging
builder.Services.AddHttpLogging(options => { options.LoggingFields = HttpLoggingFields.All; });
builder.Services.AddProblemDetails();

// Add services to the container.
builder.AddConfig();

// Add HybridCache (L1 in-memory + L2 distributed)
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

// Add Health Checks with SQL Server readiness check
var sqlConnectionString = builder.Configuration.GetConnectionString("Sql");
var healthChecksBuilder = builder.Services.AddHealthChecks();
if (sqlConnectionString != null)
{
    healthChecksBuilder.AddSqlServer(sqlConnectionString, name: "database", tags: ["ready"]);
}

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("per-user", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("global", opt =>
    {
        opt.PermitLimit = 1000;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Add Output Caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.NoCache());
    options.AddPolicy("books", policy => policy.Expire(TimeSpan.FromMinutes(5)).SetVaryByQuery("*"));
});

// Add Polly
builder.Services.AddResiliencePipeline("default",
    rp =>
    {
        rp.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 4,
            Delay = TimeSpan.FromMicroseconds(1000),
            ShouldHandle = new PredicateBuilder().Handle<Exception>()
        });
    });

// Add Http Client
builder.Services.AddHttpClient("default").AddStandardResilienceHandler();

// Add Cors
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApiWithSecurityScheme();

// Add Azure Key Vault
if (builder.Environment.IsProduction())
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());

builder.AddDbContextWithSqlConnection<BooksDbContext>();

builder.Services.AddGraphQLServer()
    .AddQueryType<BookQuery>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .InitializeOnStartup();

var app = builder.Build();

// Error handling
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/api-docs", options => { });
}

app.UseHttpLogging();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();

app.MapGraphQL().RequireAuthorization();

// Health check endpoints: liveness and readiness
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Map routes
app.MapBooksApi();

// Initialize Db with migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<IDbInitializer>();
    await db!.InitializeAsync();
}

app.Run();
