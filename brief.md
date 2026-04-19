# OAuthLab — Enterprise Architecture Agent Brief

## 1. Role

You are a senior software architect and principal backend engineer.

You are responsible for designing and implementing an enterprise-grade OAuth2 / OIDC developer tool using C# and .NET.

The project must be built with clean, maintainable, production-oriented architecture.

You must prioritize correctness, security, separation of concerns, testability, and long-term extensibility.

Do not rush implementation. Work incrementally. Prefer compiling, working code over broad unfinished scaffolding.

---

## 2. Project Goal

Build an **OAuth2 / OIDC Playground & Debugger**.

The application is a developer tool that helps users test, inspect, and understand OAuth2 / OIDC flows.

The first version is **not** a production Authorization Server.

The first version is a tool that can:

- Manage OAuth provider configurations
- Generate Authorization Code + PKCE requests
- Handle OAuth callback responses
- Exchange authorization codes for tokens
- Execute Client Credentials flow
- Refresh tokens
- Decode JWT access tokens and ID tokens
- Inspect token claims
- Store flow sessions
- Store event/audit history
- Later support Device Code flow, token introspection, token revocation, OIDC discovery, JWKS validation, and advanced enterprise flows

---

## 3. Technology Stack

Use:

- C#
- .NET 10 or latest stable .NET available
- ASP.NET Core Web API
- MongoDB
- Clean Architecture
- Domain-Driven Design
- CQRS
- Event Sourcing only where it provides clear value
- Domain Events
- Strongly typed configuration
- Dependency Injection
- FluentValidation or similar validation library
- xUnit or NUnit for tests
- Testcontainers for integration tests if appropriate
- Swagger / OpenAPI
- Typed HttpClient
- Structured logging

Do not use:

- Overly generic helper classes
- Anemic domain models where behavior belongs in domain entities
- Business logic inside controllers
- MongoDB access from API/controllers
- Unstructured dynamic dictionaries unless unavoidable for protocol payloads
- Event sourcing everywhere by default
- CQRS framework unless explicitly requested
- Event sourcing framework unless explicitly requested

---

## 4. Architectural Style

Use enterprise-grade Clean Architecture.

Recommended solution structure:

```txt
OAuthLab/
  src/
    OAuthLab.Api/
    OAuthLab.Application/
    OAuthLab.Domain/
    OAuthLab.Infrastructure/
    OAuthLab.Contracts/
  tests/
    OAuthLab.UnitTests/
    OAuthLab.IntegrationTests/
```

Optional later:

```txt
OAuthLab.Web/
OAuthLab.Worker/
```

---

## 5. Project Responsibilities

### 5.1 OAuthLab.Api

Responsible for:

- HTTP endpoints
- Request/response mapping
- Authentication/authorization later
- Minimal APIs or Controllers
- API versioning if useful
- Swagger/OpenAPI
- Global exception handling
- Validation pipeline entry point
- HTTP status code mapping
- Request correlation IDs if implemented

Must not contain:

- Business logic
- MongoDB queries
- OAuth protocol implementation details beyond request mapping
- Domain decisions
- Direct token exchange logic
- Direct event store logic

---

### 5.2 OAuthLab.Application

Responsible for:

- CQRS commands
- CQRS queries
- Command handlers
- Query handlers
- Application services
- Interfaces for repositories
- Interfaces for external OAuth HTTP clients
- DTO mapping
- Transaction/application flow orchestration
- Domain event dispatching coordination
- Use case-level validation
- Application-level authorization later

Must not contain:

- ASP.NET Core controller logic
- Concrete MongoDB code
- Infrastructure-specific persistence logic
- UI concerns

---

### 5.3 OAuthLab.Domain

Responsible for:

- Aggregates
- Entities
- Value Objects
- Domain Events
- Domain Services
- Business rules
- Invariants
- OAuth flow state model
- Provider configuration rules
- PKCE concepts
- Token metadata concepts

Must not reference:

- MongoDB
- ASP.NET Core
- EF Core
- HTTP framework
- Infrastructure projects
- Application project

---

### 5.4 OAuthLab.Infrastructure

Responsible for:

- MongoDB persistence
- Repository implementations
- Event store implementation
- OAuth HTTP client implementation
- Token endpoint HTTP calls
- Encryption/secrets storage implementation
- Time provider implementation
- External provider metadata discovery
- Logging infrastructure
- Background jobs if needed
- Mongo indexes
- Projection persistence

---

### 5.5 OAuthLab.Contracts

Responsible for:

- Public API request/response contracts
- Shared DTOs
- API-facing models

Contracts should be stable and should not expose internal domain entities directly.

---

## 6. Core Bounded Contexts

Use these bounded contexts/modules:

```txt
ProviderManagement
OAuthFlows
TokenInspection
AuditLog
```

Optional later:

```txt
DeviceAuthorization
ClientCredentials
OpenIdConnectDiscovery
UserWorkspace
```

---

## 7. Domain Model

### 7.1 ProviderManagement

#### OAuthProviderConfig

Represents an OAuth2/OIDC provider configuration.

Fields:

```txt
ProviderId
Name
AuthorizationEndpoint
TokenEndpoint
RevocationEndpoint optional
IntrospectionEndpoint optional
UserInfoEndpoint optional
Issuer optional
ClientId
ClientSecret optional
RedirectUri
DefaultScopes
SupportedGrantTypes
CreatedAt
UpdatedAt
```

Business rules:

- Name is required.
- Authorization endpoint is required for Authorization Code flow.
- Token endpoint is required for token exchange.
- Client ID is required.
- Redirect URI is required for Authorization Code flow.
- Client secret may be optional for public clients.
- Scopes must be normalized.
- Provider config must not expose raw secrets in normal query responses.
- Provider names should be unique.
- Endpoint values must be valid absolute URLs.
- Default scopes should not contain duplicates.

#### ProviderManagement Value Objects

Use value objects where meaningful:

```txt
ProviderId
ProviderName
OAuthEndpoint
ClientId
ClientSecret
RedirectUri
ScopeCollection
GrantType
```

---

### 7.2 OAuthFlows

#### OAuthFlowSession

Represents one OAuth flow attempt/session.

This is a good candidate for Event Sourcing.

Fields/state:

```txt
FlowSessionId
ProviderId
FlowType
State
PkceVerifier
PkceChallenge
AuthorizationUrl
CallbackCode
CallbackError
TokenResponseId
Status
CreatedAt
CompletedAt
FailedAt
```

Possible statuses:

```txt
Created
AuthorizationUrlGenerated
CallbackReceived
TokenExchangeCompleted
Failed
Expired
```

Business rules:

- A flow session starts in Created state.
- PKCE verifier must be generated before Authorization Code + PKCE URL is produced.
- Callback must validate state.
- Token exchange cannot happen before callback code is received.
- A failed session cannot be exchanged unless explicitly retried through a new session.
- Expired sessions cannot be completed.
- Sensitive values must be protected.
- Flow state must be unpredictable.
- Flow state must be matched during callback handling.

#### OAuthFlowSession Domain Events

For OAuthFlowSession:

```txt
OAuthFlowSessionStarted
PkceChallengeGenerated
AuthorizationUrlGenerated
OAuthCallbackReceived
OAuthCallbackFailed
AuthorizationCodeExchangedForToken
OAuthFlowSessionFailed
OAuthFlowSessionExpired
```

Use Event Sourcing here if it remains manageable.

---

### 7.3 TokenInspection

#### TokenResponseRecord

Represents a token response captured during a flow.

Fields:

```txt
TokenResponseId
FlowSessionId
ProviderId
AccessToken
RefreshToken optional
IdToken optional
TokenType
ExpiresIn
Scope
RawResponse
CreatedAt
```

Security rules:

- Raw tokens are sensitive.
- Do not expose full tokens by default.
- Mask tokens in query responses.
- Provide explicit reveal functionality only if intentionally designed.
- Consider encrypting tokens at rest.
- Never log full token values.
- Never include full token values in application errors.

#### DecodedJwt

Represents decoded JWT header/payload.

Fields:

```txt
Algorithm
KeyId
Issuer
Audience
Subject
Expiration
IssuedAt
NotBefore
Scopes
Claims
RawHeader
RawPayload
```

Business rules:

- Decode does not necessarily mean validate.
- Clearly distinguish “decoded” from “validated”.
- Token validation can be added later with JWKS discovery.
- Invalid JWT format should produce a clean validation error.

---

### 7.4 AuditLog

#### AuditEvent

Represents important system/user actions.

Can be event-sourced or append-only.

Events:

```txt
ProviderConfigCreated
ProviderConfigUpdated
ProviderConfigDeleted
OAuthFlowStarted
TokenExchangePerformed
TokenDecoded
RefreshTokenUsed
ClientCredentialsTokenRequested
```

Audit log should be append-only.

---

## 8. CQRS Requirements

Use CQRS explicitly.

Commands mutate state.

Queries read state.

Suggested structure:

```txt
OAuthLab.Application/
  ProviderManagement/
    Commands/
      CreateProviderConfig/
      UpdateProviderConfig/
      DeleteProviderConfig/
    Queries/
      GetProviderConfigById/
      ListProviderConfigs/

  OAuthFlows/
    Commands/
      StartAuthorizationCodePkceFlow/
      HandleOAuthCallback/
      ExchangeAuthorizationCodeForToken/
      RefreshAccessToken/
      ExecuteClientCredentialsFlow/
    Queries/
      GetFlowSessionById/
      ListFlowSessions/

  TokenInspection/
    Commands/
      DecodeJwtToken/
    Queries/
      GetTokenResponseById/
      ListTokenResponses/

  AuditLog/
    Queries/
      ListAuditEvents/
```

Each command/query should have:

```txt
Request model
Handler
Validator
Result/Response model
```

Example naming:

```txt
CreateProviderConfigCommand
CreateProviderConfigCommandHandler
CreateProviderConfigCommandValidator
CreateProviderConfigResult
```

Rules:

- Command handlers should not return domain entities directly.
- Query handlers should return read models or DTOs.
- Controllers should not call repositories directly.
- Controllers should call command/query handlers through a clear application boundary.
- Avoid introducing MediatR unless explicitly requested.
- If no mediator is used, use direct DI of command/query handlers.

---

## 9. Event Sourcing Guidance

Do not event-source every aggregate.

Use Event Sourcing where state transitions matter and history is valuable.

Recommended Event Sourced aggregate:

```txt
OAuthFlowSession
```

Reason:

- Flow state transitions are important.
- Debugging OAuth flow history is valuable.
- Timeline reconstruction is useful.
- Mistakes, failed callbacks, and token exchange attempts should be inspectable.

Do not initially event-source:

```txt
OAuthProviderConfig
TokenResponseRecord
DecodedJwt
```

These can be regular MongoDB document models.

Use an append-only MongoDB collection for events:

```txt
event_store
```

Suggested document:

```json
{
  "_id": "ObjectId",
  "streamId": "oauth-flow-session-...",
  "streamType": "OAuthFlowSession",
  "version": 3,
  "eventType": "OAuthCallbackReceived",
  "payload": {},
  "metadata": {
    "correlationId": "...",
    "causationId": "...",
    "userId": "...",
    "ipAddress": "...",
    "userAgent": "..."
  },
  "occurredAt": "2026-04-19T20:00:00.000Z"
}
```

Requirements:

- Events are append-only.
- Use optimistic concurrency with stream version.
- Aggregate is rebuilt from event stream.
- New events are appended after command handling.
- Read models/projections may be updated after events are appended.
- Event payloads should be version-tolerant.
- Event type names should be stable.
- Do not store sensitive token values in events unless explicitly justified and protected.

Projection collections:

```txt
oauth_flow_sessions
audit_events
```

---

## 10. MongoDB Collections

Use MongoDB as the primary persistence layer.

Suggested collections:

```txt
provider_configs
oauth_flow_sessions
token_responses
event_store
audit_events
```

Optional later:

```txt
jwks_cache
oidc_discovery_documents
user_workspaces
```

### 10.1 provider_configs

Stores provider settings.

Important:

- Client secrets must be encrypted or protected.
- Query responses must mask secrets.

Indexes:

```txt
unique index on name
index on issuer
index on createdAt
```

### 10.2 oauth_flow_sessions

Projection/read model for OAuth flow sessions.

This may be derived from event store.

Indexes:

```txt
index on providerId
index on status
index on createdAt
index on flowType
```

### 10.3 token_responses

Stores token responses captured during testing.

Indexes:

```txt
index on flowSessionId
index on providerId
index on createdAt
```

Security:

- Access token, refresh token, and ID token must be treated as secrets.
- Mask in list/detail responses unless explicitly needed.
- Consider encryption at rest.

### 10.4 event_store

Append-only event collection.

Indexes:

```txt
unique compound index on streamId + version
index on streamId
index on streamType
index on eventType
index on occurredAt
```

### 10.5 audit_events

Append-only audit projection.

Indexes:

```txt
index on eventType
index on occurredAt
index on correlationId
```

---

## 11. Main Features and Phases

### Phase 1 — Foundation

Implement only:

- Solution structure
- Project references
- Health endpoint
- MongoDB connection
- Strongly typed configuration
- Global error handling
- Swagger
- Basic result/error model
- Basic logging

Acceptance criteria:

- Solution builds.
- API starts.
- Health endpoint works.
- MongoDB connection is configured.
- No business logic exists in API project.
- Swagger is available.
- README explains how to run the project.

---

### Phase 2 — Provider Config Management

Implement:

- Create provider config
- Update provider config
- Delete provider config
- Get provider config by ID
- List provider configs
- Secret masking
- Validation

Acceptance criteria:

- Provider config CRUD works.
- Client secret is not returned in normal responses.
- Application layer owns command/query handlers.
- Infrastructure layer owns Mongo implementation.
- Provider config validation prevents invalid endpoint URLs.
- Provider names are unique.

---

### Phase 3 — PKCE + Authorization URL

Implement:

- PKCE verifier generation
- PKCE challenge generation with S256
- Start Authorization Code + PKCE flow
- Generate authorization URL
- Store OAuth flow session
- Append domain events for flow session

Acceptance criteria:

- User can start flow for a provider.
- System returns authorization URL.
- Flow session is stored.
- Event store contains flow events.
- State value is generated and stored.
- PKCE verifier is stored securely enough for local dev.
- Authorization URL contains correct OAuth parameters.

---

### Phase 4 — Callback Handling

Implement:

- Callback endpoint
- State validation
- Capture authorization code
- Capture OAuth error
- Update flow session
- Append callback event

Acceptance criteria:

- Successful callback stores code.
- Failed callback stores error.
- Invalid state is rejected.
- Flow session status changes correctly.
- Event history is preserved.

---

### Phase 5 — Token Exchange

Implement:

- Exchange authorization code for token
- Typed HTTP client for OAuth token endpoint
- Store token response
- Decode JWT tokens if present
- Append token exchange event

Acceptance criteria:

- Token endpoint request is correct.
- Token response is stored.
- Sensitive tokens are masked in normal API responses.
- JWTs are decoded but not falsely marked as validated.
- Flow session status becomes completed.

---

### Phase 6 — Client Credentials Flow

Implement:

- Execute client credentials flow
- Send client ID/secret
- Send requested scopes
- Store token response
- Decode JWT access token if applicable

Acceptance criteria:

- Machine-to-machine token request works.
- Token response is stored.
- Query responses mask sensitive token values.
- Errors from provider are captured cleanly.

---

### Phase 7 — Refresh Token Flow

Implement:

- Refresh access token
- Store refreshed token response
- Link it to original flow session if applicable
- Append audit event

Acceptance criteria:

- Refresh token request works.
- New token response is stored.
- Refresh token is masked.
- Errors are captured cleanly.

---

### Phase 8 — JWT Decode / Token Inspection

Implement:

- Decode JWT header
- Decode JWT payload
- Show claims
- Show expiration status
- Show issuer/audience/scope/subject
- Clearly label token as decoded, not validated

Acceptance criteria:

- Invalid JWT returns validation error.
- Valid JWT is decoded.
- No validation claim is made unless signature validation exists.

---

### Phase 9 — Tests

Add tests for:

- PKCE generation
- Authorization URL generation
- Flow state transitions
- Event sourced aggregate rebuild
- Provider config validation
- JWT decoding
- Mongo repository integration tests where appropriate

Acceptance criteria:

- Unit tests cover domain behavior.
- Integration tests cover persistence basics.
- Tests are not just snapshot/scaffold tests.
- Tests are meaningful and runnable.

---

## 12. API Endpoint Suggestions

Provider configs:

```txt
POST   /api/provider-configs
GET    /api/provider-configs
GET    /api/provider-configs/{id}
PUT    /api/provider-configs/{id}
DELETE /api/provider-configs/{id}
```

OAuth flows:

```txt
POST /api/oauth-flows/authorization-code-pkce/start
GET  /api/oauth-flows/{id}
GET  /api/oauth-flows
GET  /api/oauth/callback
POST /api/oauth-flows/{id}/exchange-code
POST /api/oauth-flows/client-credentials
POST /api/oauth-flows/{id}/refresh-token
```

Token inspection:

```txt
POST /api/tokens/decode
GET  /api/token-responses/{id}
GET  /api/token-responses
```

Audit:

```txt
GET /api/audit-events
```

Health:

```txt
GET /api/health
```

---

## 13. Security Requirements

This is a developer tool, but still follow secure defaults.

Rules:

- Never log full access tokens.
- Never log full refresh tokens.
- Never return client secret in normal responses.
- Mask sensitive values.
- Use HTTPS assumptions in documentation.
- Store state and validate it.
- Use PKCE S256.
- Do not support plain PKCE unless explicitly required later.
- Do not treat decoded JWT as validated JWT.
- Add warnings where token validation is not performed.
- Avoid storing secrets in plain text if encryption is feasible.
- Use configuration for encryption key if implementing encrypted storage.
- Avoid writing tokens into event payloads.
- Avoid writing tokens into audit events.
- Avoid leaking provider error details that contain secrets.
- Use cancellation tokens in async operations.
- Use structured logs and avoid sensitive values.

Token masking example:

```txt
eyJhbGciOi...Q9xA
```

or:

```txt
first 8 chars + last 4 chars
```

---

## 14. Coding Rules

- Use nullable reference types.
- Use explicit models.
- Avoid `dynamic`.
- Avoid `object` payloads except event payload serialization boundaries.
- Keep methods small.
- Keep command handlers focused.
- Keep controllers thin.
- Use cancellation tokens.
- Use typed HttpClient.
- Use options pattern for settings.
- Use structured logging.
- Prefer Result pattern or clear domain/application exceptions.
- Do not leak internal exception details to HTTP clients.
- Avoid generic names like `Helper`, `Manager`, `Processor` unless the role is precise.
- Do not create abstractions without clear purpose.
- Prefer simple internal abstractions before adding frameworks.
- Do not return domain entities directly from API endpoints.
- Keep API contracts separate from domain models.

---

## 15. Implementation Discipline

Do not implement all phases at once.

Start with Phase 1 only.

After each phase:

- Ensure solution builds.
- Ensure tests pass.
- Summarize what changed.
- List next recommended phase.
- Do not add unrelated features.
- Do not introduce CQRS frameworks unless explicitly asked.
- Do not introduce event sourcing frameworks unless explicitly asked.
- Implement simple internal abstractions first.
- Prefer working software over broad unfinished scaffolding.

---

## 16. First Implementation Task

Start with only this:

```txt
Phase 1 — Foundation
```

Create:

```txt
OAuthLab.Api
OAuthLab.Application
OAuthLab.Domain
OAuthLab.Infrastructure
OAuthLab.Contracts
OAuthLab.UnitTests
OAuthLab.IntegrationTests
```

Implement:

```txt
GET /api/health
MongoDB connection setup
Strongly typed configuration
Global error handling
Swagger
Basic Result/Error model
Project references
README with run instructions
```

Do not implement OAuth flows yet.

Do not implement ProviderConfig yet.

Do not implement event sourcing yet.

Make the solution compile and run first.

---

## 17. Initial Agent Prompt

Use this as the first instruction to the coding agent:

```txt
Use the attached architecture brief as the target architecture.

Important:
Do not implement all phases at once.

Implement only Phase 1 — Foundation.

The goal is a compiling, running .NET solution with:
- Clean Architecture project structure
- Health endpoint
- MongoDB connection setup
- Strongly typed configuration
- Swagger
- Global error handling
- Basic Result/Error model
- README

Do not implement OAuth flows yet.
Do not implement ProviderConfig yet.
Do not implement event sourcing yet.
Do not add unnecessary abstractions.

After implementation, summarize changed files and how to run the project.
```

---

## 18. Future Features

Optional future features:

- Device Authorization Flow
- Token introspection
- Token revocation
- OIDC discovery document support
- JWKS cache
- JWT signature validation
- ID token validation
- UserInfo endpoint call
- Provider templates for common providers
- Workspace/project support
- Saved flow runs
- Flow timeline UI
- Export flow debug report
- Environment variables/secrets vault support
- Docker Compose setup
- Web UI
- CLI client
- Go implementation later
- Mini Authorization Server module later

---

## 19. Non-Goals for Initial Version

Do not implement initially:

- Production Authorization Server
- User login system for OAuthLab itself
- Multi-tenant workspace model
- Complex permission system
- Full OIDC conformance
- Consent screen
- User management
- Authorization policy engine
- Distributed event bus
- Kafka/RabbitMQ
- Full event sourcing for every aggregate
- Complex frontend
- Cloud deployment scripts

---

## 20. Good MVP Definition

A good MVP is:

- The API starts.
- Provider configs can be managed.
- Authorization Code + PKCE URL can be generated.
- Callback can be captured.
- Authorization code can be exchanged for tokens.
- JWTs can be decoded.
- Client Credentials flow can be tested.
- Flow history can be inspected.
- Sensitive values are masked.
- Code remains understandable and extensible.

The first milestone is much smaller:

- Clean solution structure
- Health endpoint
- MongoDB connection
- Swagger
- Global error handling
- Basic Result/Error model

Keep the first implementation small and working.
