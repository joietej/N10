# **Building Resilient, Secure, and Scalable APIs with ASP.NET Core 10 Minimal APIs**

This guide provides a comprehensive approach to building production-ready APIs using **ASP.NET Core 10 Minimal APIs**, aligned with the **Azure Well-Architected Framework** and **Cloud Design Patterns**. Each section dives into key aspects such as reliability, performance efficiency, security, cost optimization, and observability, ensuring you have the tools to create robust solutions.

## **Introduction**

Minimal APIs in ASP.NET Core 10 offer a lightweight, flexible approach to building modern web APIs. By focusing on simplicity and performance, they provide an efficient way to create scalable solutions for cloud-native applications. Leveraging integrations with Azure services such as Azure Front Door, Azure Container Apps, and Azure Monitor, you can design resilient APIs that adhere to best practices and architectural patterns.

## **What's New in .NET 10 and ASP.NET Core 10**

.NET 10 and ASP.NET Core 10 introduce several enhancements and new features aimed at improving performance, productivity, and scalability for modern application development. Here are some key improvements:

### **Key Improvements in .NET 10**

1. **Enhanced Native AOT (Ahead-of-Time Compilation)**:
   - Further improvements to application startup performance and reduced memory usage.
   - Broader library compatibility with Native AOT for minimal APIs and microservices.

2. **Performance Enhancements**:
   - Improved garbage collection (GC) for better memory management.
   - Enhanced Just-In-Time (JIT) compiler optimizations.
   - Improved thread pool and async performance.

3. **Enhanced OpenTelemetry Support**:
   - Native integration for distributed tracing and metrics collection.
   - Simplified observability for cloud-native applications.

4. **Updated OpenAPI Support**:
   - Microsoft.OpenApi v2.0 with restructured API models.
   - Improved document transformers for OpenAPI customization.

5. **Entity Framework Core 10**:
   - Performance improvements for query execution.
   - Enhanced migration tooling.

### **Key Improvements in ASP.NET Core 10**

1. **Typed Results for Minimal APIs**:
   - Improved type safety and reduced boilerplate code when returning responses.
   - Example:
     ```csharp
     app.MapGet("/user", () => TypedResults.Ok(new { Name = "John", Age = 30 }));
     ```

2. **Rate Limiting Middleware**:
   - Built-in middleware for controlling traffic and protecting APIs.
   - Simplifies adding rate-limiting policies to endpoints.

3. **Improved Authentication and Authorization**:
   - Enhanced integration with Azure Active Directory and OpenID Connect.
   - Simplified token validation and role-based access control (RBAC).

4. **HTTP Logging Enhancements**:
   - Allows detailed logging of HTTP requests and responses for debugging.
   - Supports filtering and customization of logged data.

5. **Enhanced Deployment Options**:
   - Better support for Azure Container Apps and other containerized environments.
   - Simplified configuration for Kubernetes and cloud-native deployments.

6. **Deprecated WithOpenApi() Extension**:
   - The `WithOpenApi()` extension method is deprecated in .NET 10.
   - OpenAPI metadata is now automatically generated from endpoint metadata.

### **Why These Improvements Matter**

These enhancements in .NET 10 and ASP.NET Core 10 help developers:
- Build high-performance, scalable applications.
- Simplify application monitoring, security, and deployment.
- Align with modern cloud-native practices and frameworks like the Azure Well-Architected Framework.

By leveraging these improvements, developers can create robust, secure, and efficient APIs that meet modern application demands.



## **Azure Well-Architected Framework**

The **Azure Well-Architected Framework** provides a set of best practices and guiding principles to design high-quality cloud-native solutions. It focuses on five key pillars:
1. **Reliability**: Ensures the application can recover from failures and continue functioning.
2. **Security**: Protects applications and data from threats.
3. **Cost Optimization**: Manages and reduces operational expenses.
4. **Operational Excellence**: Improves deployment processes and operational workflows.
5. **Performance Efficiency**: Ensures efficient resource use and optimized performance.

This document aligns with these principles by:
- **Reliability**: Using Polly for resilience, database retry logic, and rate limiting.
- **Security**: Implementing Azure Front Door, Key Vault, and Azure AD for protection.
- **Cost Optimization**: Leveraging Azure Container Apps for cost-effective scaling.
- **Performance Efficiency**: Utilizing HybridCache and OpenTelemetry to optimize performance.
- **Operational Excellence**: Following best practices for deployment, monitoring, and scaling APIs.

By following this guide, you'll have the foundational knowledge to create secure, scalable, and cost-efficient APIs while adhering to Azure Well-Architected Framework principles.



# **1. OpenAPI Documentation with Scalar**

**OpenAPI** documentation provides a standard for describing APIs, making them easier to understand and consume. **Scalar** simplifies hosting and sharing OpenAPI schemas.

## **Key Features**
- **Version Control**: Manage multiple versions of API documentation.
- **Collaboration**: Centralized hub for internal and external teams.
- **API Discovery**: Simplifies onboarding by making APIs accessible.

## **Implementation Steps**

### Enable OpenAPI in Minimal APIs

```csharp
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithEndpointPrefix("/api-docs/{documentName}");
    });
}
```

### Customize OpenAPI with Document Transformers

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, token) =>
    {
        var components = document.Components ?? new OpenApiComponents();
        document.Components = components;
        components.SecuritySchemes!.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });

        return Task.CompletedTask;
    });
});
```


# **2. Reliability**

Reliability ensures that APIs remain operational during failures, high loads, or transient errors.

## **Key Features**
- **Rate Limiting**: Protects backend services from being overwhelmed.
- **Resilience with Polly**: Adds retries, timeouts, and circuit breakers for external dependencies.
- **Database Resilience**: Handles transient database failures using retry logic.

## **Implementation Steps**

### Rate Limiting

Add rate limiting to control traffic and prevent overload:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("Fixed", config =>
    {
        config.Window = TimeSpan.FromSeconds(10);
        config.PermitLimit = 5;
        config.QueueLimit = 2;
    });
});

app.UseRateLimiter();

app.MapGet("/rate-limited", () => "Rate limited endpoint!")
   .RequireRateLimiter("Fixed");
```

### Resilience with Polly

Handle transient failures with retries using the resilience pipeline:

```csharp
builder.Services.AddResiliencePipeline("default", rp =>
{
    rp.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 4,
        Delay = TimeSpan.FromMicroseconds(1000),
        ShouldHandle = new PredicateBuilder().Handle<Exception>()
    });
});

builder.Services.AddHttpClient("default").AddStandardResilienceHandler();
```

### Database Resilience

Add retry logic to handle database deadlocks and transient issues:

```csharp
builder.Services.AddDbContext<BooksDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));
```


# **3. Security**

Security is critical to protecting APIs from unauthorized access, malicious traffic, and data breaches.

## **Key Features**
- **Azure Front Door**: Protects APIs with DDoS protection, WAF, and HTTPS enforcement.
- **Authentication**: Uses Azure AD with JWT tokens for secure API access.
- **Secrets Management**: Securely stores sensitive data in Azure Key Vault.

## **Implementation Steps**

### Authentication with Azure AD

Use Azure AD for secure access to APIs with JWT tokens:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/secure-endpoint", () => "This endpoint is secured.")
   .RequireAuthorization();
```

### Secrets Management with Azure Key Vault

Store sensitive data such as connection strings and API keys securely:

```csharp
if (builder.Environment.IsProduction())
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
```


# **4. Performance Efficiency**

Optimizing the performance of APIs ensures faster response times, reduced latency, and a better user experience.

## **Key Features**
- **HybridCache**: Combines in-memory and distributed caching for high-speed data retrieval.
- **Metrics Collection with OpenTelemetry**: Tracks performance and resource usage.
- **GraphQL with HotChocolate**: Efficient data fetching with projections, filtering, and sorting.

## **Implementation Steps**

### GraphQL with HotChocolate

Leverage GraphQL for flexible data querying:

```csharp
builder.Services.AddGraphQLServer()
    .AddQueryType<BookQuery>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .InitializeOnStartup();

app.MapGraphQLHttp().RequireAuthorization();
app.MapNitroApp("/graphql/ui");
```

### Metrics Collection with OpenTelemetry

Track API metrics using OpenTelemetry:

```csharp
var otel = builder.Services.AddOpenTelemetry();

otel.WithMetrics(metrics => { metrics.AddAspNetCoreInstrumentation(); });

otel.WithTracing(trace =>
{
    trace.AddAspNetCoreInstrumentation();
    trace.AddHttpClientInstrumentation();
});

var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (otlpEndpoint != null) otel.UseOtlpExporter();

var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (appInsightsConnectionString != null) otel.UseAzureMonitor();
```


# **5. Observability**

Observability is essential for monitoring API behavior, troubleshooting issues, and ensuring high availability.

## **Key Features**
- **Distributed Tracing with OpenTelemetry**: Captures traces for monitoring API behavior.
- **Metrics Collection**: Tracks API latency, throughput, and error rates.
- **HTTP Logging**: Captures detailed HTTP request and response data for debugging.

## **Implementation Steps**

### HTTP Logging for Debugging

Enable detailed logging of HTTP requests and responses:

```csharp
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
});

app.UseHttpLogging();
```

### OpenTelemetry Logging

Add structured logging with OpenTelemetry:

```csharp
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
});
```


# **6. Exception Handling**

Effective exception handling ensures that APIs provide meaningful error responses while maintaining stability and security.

## **Key Features**
- **Status Code Pages**: Returns standardized problem details for error status codes.
- **Developer Exception Page**: Detailed error information in development.
- **Production Exception Handler**: Sanitized error responses in production.

## **Implementation Steps**

### Exception Handling Extension

```csharp
public static class ErrorHandlingExtensions
{
    public static WebApplication UseExceptionHandling(this WebApplication app)
    {
        app.UseStatusCodePages(async statusCodeContext
            => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
                .ExecuteAsync(statusCodeContext.HttpContext));

        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler(a =>
            {
                a.Run(async ctx => await Results.Problem().ExecuteAsync(ctx));
            });

        return app;
    }
}
```


# **7. Cost Optimization**

Cost optimization focuses on reducing operational costs and resource usage without compromising performance or reliability.

## **Implementation Steps**

### Containerize the Application

#### Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["N10.WebApi/N10.WebApi.csproj", "N10.WebApi/"]
RUN dotnet restore "N10.WebApi/N10.WebApi.csproj"
COPY . .
WORKDIR "/src/N10.WebApi"
RUN dotnet build "N10.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "N10.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "N10.WebApi.dll"]
```

#### Deploy to Azure Container Apps

```bash
az containerapp create \
  --name n10-api \
  --resource-group my-resource-group \
  --image myregistry.azurecr.io/n10-api:latest \
  --cpu 0.5 --memory 1.0Gi \
  --min-replicas 1 --max-replicas 5 \
  --environment my-environment
```


# **8. Architecture Blueprint**

## **Key Components**
- **Azure Front Door**: For global traffic distribution and enhanced security with DDoS protection.
- **Scalar**: API documentation and interactive testing interface.
- **GraphQL (HotChocolate)**: Flexible data querying with projections and filtering.
- **Polly**: Adds resilience with retries, timeouts, and circuit breakers.
- **OpenTelemetry**: Tracks distributed traces and collects metrics for observability.
- **Azure Monitor**: Provides centralized monitoring and diagnostics.

## **Architecture Diagram**

```plaintext
[Client] --> [Azure Front Door] --> [Minimal API (.NET 10)]
                                |--> [Azure SQL Database]
                                |--> [GraphQL (HotChocolate)]
                                |--> [Azure Monitor + OpenTelemetry]
                                |--> [Azure Key Vault]
```

## **Summary Steps**
1. Deploy the Minimal API to **Azure Container Apps** for scalability and cost efficiency.
2. Integrate **OpenTelemetry** for distributed tracing and metrics collection.
3. Configure **GraphQL** for flexible data querying.
4. Secure APIs using **Azure Front Door** and **Azure AD** for traffic routing and authentication.
5. Monitor and optimize using **Azure Monitor** and **Application Insights**.

## **Links and References**

### **ASP.NET Core 10 and Related Resources**
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Minimal APIs Overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [.NET 10 Download](https://dotnet.microsoft.com/download/dotnet/10.0)

### **OpenAPI and Scalar**
- [Scalar - API Documentation](https://scalar.com/)
- [OpenAPI Specification](https://swagger.io/specification/)

### **GraphQL**
- [HotChocolate GraphQL](https://chillicream.com/docs/hotchocolate)

### **Reliability**
- [Polly - Resilience Framework](https://github.com/App-vNext/Polly)
- [Rate Limiting in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limiting)
- [EF Core Retry Logic](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)

### **Security**
- [Azure Front Door](https://learn.microsoft.com/en-us/azure/frontdoor/)
- [Azure Active Directory (Azure AD)](https://learn.microsoft.com/en-us/azure/active-directory/)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)

### **Performance and Observability**
- [OpenTelemetry for .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Azure Monitor](https://learn.microsoft.com/en-us/azure/azure-monitor/)
- [HTTP Logging Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-logging)

### **Cost Optimization**
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Docker for Containerization](https://www.docker.com/)

### **Azure Well-Architected Framework**
- [Azure Well-Architected Framework Overview](https://learn.microsoft.com/en-us/azure/architecture/framework/)
