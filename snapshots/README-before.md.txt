# NetTrello

NetTrello is a Trello-style kanban starter built with C#, ASP.NET Core, React, and TypeScript. It is intentionally structured as a teaching codebase: the app is small enough to read in one sitting, but it uses patterns that scale into production systems.

## What Is Included

- ASP.NET Core API with domain entities, application services, typed contracts, endpoint mapping, centralized errors, security headers, CORS, health checks, and optimistic concurrency.
- React client with TypeScript contracts, a dedicated API client, a board orchestration hook, reusable components, card editing, card archiving, board creation, column creation, and drag/drop card movement.
- In-memory persistence seeded with sample data so the product can run before a database is introduced.
- Production-oriented defaults: nullable C#, warnings as errors, code style enforcement, Dockerfile, environment example, and architecture docs.

## Project Layout

```text
src/NetTrello.Api/
  Domain/          Business rules and entities
  Application/     Use cases and persistence abstractions
  Contracts/       HTTP request and response DTOs
  Endpoints/       Minimal API route definitions
  Infrastructure/  Replaceable implementations such as in-memory storage
  Middleware/      Cross-cutting HTTP behavior

client/src/
  api/             Fetch wrapper and API methods
  components/      Focused React UI components
  hooks/           State orchestration and side effects
  styles/          App-level CSS
```

## Run Locally

This machine does not currently have `dotnet` on PATH, so I could not execute these commands here. Once the .NET SDK is available:

```powershell
dotnet run --project src/NetTrello.Api
```

In another terminal:

```powershell
cd client
npm install
npm run dev
```

Open `http://localhost:5173`. The client expects the API at `http://localhost:5088`; override it with `VITE_API_BASE_URL` if needed.

## Senior Engineering Notes

- The API does not expose domain objects directly. `Contracts/` holds the HTTP shape, and `BoardMapper` translates domain state into response DTOs.
- Mutations accept `X-Board-Version`. This models optimistic concurrency, which prevents silent overwrites when two users edit the same board.
- Persistence is behind `IBoardRepository`. Replacing the in-memory repository with EF Core, Dapper, or a document store should not change endpoints or React code.
- Errors are normalized as RFC 7807 problem details with stable `code` values, making client behavior testable and user messages consistent.
- React components are deliberately thin. `useBoard` owns orchestration, `boardClient` owns HTTP, and components focus on rendering and local form state.

## Next Production Steps

- Add authentication and authorization policies around boards and memberships.
- Replace `InMemoryBoardRepository` with durable persistence and migration scripts.
- Add integration tests for endpoints and component tests for drag/drop workflows.
- Add audit logging for board mutations and background jobs for notifications.
