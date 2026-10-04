# SwipeSwap

SwipeSwap is a mobile-first barter exchange platform that allows users to trade items directly without using money.
The backend is built with ASP.NET Core (.NET 9), following DDD and Clean Architecture principles, and uses PostgreSQL as the main database, Redis, and JWT authentication.

## Structure
- `src/SwipeSwap.WebAPI` — host (startup, middleware, validators)
- `src/SwipeSwap.EntryPoint` — REST controllers (`/api/v1/...`) and DTOs
- `src/SwipeSwap.Application` — MediatR handlers
- `src/SwipeSwap.Domain` — entities and exceptions
- `src/SwipeSwap.Infrastructure.{Postgres,Redis,Jwt}` — infrastructure
- `tests/` — unit tests (xunit, Moq); integration tests are a placeholder

## Requirements
.NET SDK 9.0, Docker (for PostgreSQL and Redis).

## Run with Docker Compose
```
docker compose up --build
```
API: http://localhost:8080, Swagger UI: http://localhost:8080/swagger

## Run locally
```
docker compose up -d postgres redis
dotnet run --project src/SwipeSwap.WebAPI
```
API: http://localhost:5007 (see `launchSettings.json`). Migrations are applied on startup.

## Configuration
Settings live in `src/SwipeSwap.WebAPI/appsettings.json` and can be overridden by environment variables:

| Setting | Env var |
|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `ConnectionStrings:Redis` | `ConnectionStrings__Redis` |
| `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpireMinutes` | `Jwt__Key`, ... |

## API (`/api/v1`)
- `authenticate`: `POST register`, `POST login`, `POST refresh`
- `user`: `GET {id}`, `GET me`, `PUT me`
- `items`: catalog, recommended, CRUD for own items
- `exchanges`: create, get, accept, decline, cancel, complete
- `chat`: list chats, create chat, get/send messages

## Tests
```
dotnet test
```
