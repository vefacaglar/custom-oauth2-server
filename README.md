# OAuthLab — OAuth2 / OIDC Playground & Debugger

An enterprise-grade developer tool for testing, inspecting, and understanding OAuth2 / OIDC flows.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- [MongoDB](https://www.mongodb.com/try/download/community) running locally on `localhost:27017`

## Project Structure

```
OAuthLab/
  src/
    OAuthLab.Api/              — HTTP endpoints, middleware, Swagger/OpenAPI
    OAuthLab.Application/      — Commands, queries, interfaces, configuration
    OAuthLab.Domain/           — Aggregates, entities, value objects, domain events
    OAuthLab.Infrastructure/   — MongoDB persistence, HTTP clients, external services
    OAuthLab.Contracts/        — Public API request/response DTOs
  tests/
    OAuthLab.UnitTests/        — Unit tests
    OAuthLab.IntegrationTests/ — Integration tests
```

## Running the Project

1. Start MongoDB via Docker Compose:
   ```bash
   docker compose up -d
   ```
   MongoDB will be available on port `27018` (not default 27017).

2. From the solution root, run:
   ```bash
   dotnet run --project src/OAuthLab.Api
   ```

3. The API will be available at:
   - HTTPS: `https://localhost:5001`
   - HTTP: `http://localhost:5000`
   - Health check: `GET /api/health`
   - OpenAPI: `https://localhost:5001/openapi/v1.json`

## Configuration

Edit `src/OAuthLab.Api/appsettings.json` to configure MongoDB connection:

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "oauthlab"
  }
}
```

## Building

```bash
dotnet build OAuthLab.slnx
```

## Testing

```bash
dotnet test OAuthLab.slnx
```

## Architecture

- Clean Architecture with DDD principles
- CQRS pattern (commands and queries)
- Event Sourcing for OAuthFlowSession aggregate
- MongoDB as primary persistence
- Structured logging
