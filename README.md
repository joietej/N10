# N10

![License](https://img.shields.io/badge/license-MIT-blue.svg)

**N10** is a .NET 10 Minimal Web API application emphasizing scalability, resilience, and security.

## Overview

This project demonstrates a structured approach to building a web API using .NET 10, incorporating:

- **Data Layer**: Utilizes Entity Framework Core 10 with SQL Server for data management and migrations.
- **Service Layer**: Contains business logic interfacing between the API and data layers.
- **API Layer**: Exposes endpoints, integrates middleware, and manages authentication and authorization.

## Features

- **Entity Framework Core 10**: Database interactions and migrations with SQL Server.
- **OpenAPI (Scalar)**: API documentation and testing interface.
- **GraphQL (HotChocolate)**: GraphQL endpoint with projections, filtering, and sorting.
- **Polly**: Implements resilience strategies like retries and circuit breakers.
- **OpenTelemetry**: Distributed tracing, metrics, and logging.
- **Middleware**:
    - HTTP request logging.
    - Global exception handling with standardized problem details.
- **Authentication & Authorization**:
    - Azure Active Directory integration.
    - JWT-based authentication.

## Project Structure

```plaintext
├── src/
│   ├── N10.Data/            # Data access layer (EF Core, repositories)
│   ├── N10.Services/        # Business logic layer
│   └── N10.WebApi/          # API layer (endpoints, middleware, config)
├── tests/                   # Unit and integration tests
├── N10.sln                  # Solution file
└── README.md                # Project overview
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- Docker (optional)
- Azure Active Directory setup for authentication

### Setup Instructions

1. **Configure the database connection string**:

   Update the `ConnectionStrings:Sql` in `appsettings.Development.json` with your SQL Server details.

2. **Run the application**:

   ```bash
   dotnet run --project src/N10.WebApi
   ```

   The application will automatically apply migrations and seed the database on startup.

3. **Access the API documentation**:

    Navigate to `http://localhost:5295/api-docs/v1` in your browser.

4. **Access the GraphQL UI**:

    Navigate to `http://localhost:5295/graphql/ui` in your browser.

## Run as a Docker Container

To run the API as a Docker container:

```bash
docker build -t N10-api -f src/N10.WebApi/Dockerfile src/
docker run -d -p 5295:8080 --name N10-api N10-api
```

## Configuration

### Authentication

Configure Azure AD settings in `appsettings.json`:

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "your-tenant-id",
  "ClientId": "your-client-id"
}
```

### Middleware

- **Polly**: Configured for transient fault handling with retry strategies.
- **HTTP Logging**: Enabled for request and response logging.
- **Exception Handling**: Standardized error responses using Problem Details.
- **OpenTelemetry**: Distributed tracing and metrics with OTLP exporter and Azure Monitor support.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/books` | Get all books |
| GET | `/healthz` | Health check |
| POST | `/graphql` | GraphQL endpoint (requires auth) |

## Running Tests

```bash
dotnet test N10.sln
```

## License

This project is licensed under the MIT License.
