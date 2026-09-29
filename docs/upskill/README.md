# NetTrello Upskill Lab

NetTrello is a compact Trello-style training repo with one ASP.NET Core API project and one Vite React client. The API starts in `src/NetTrello.Api/Program.cs:7-39`, exposes board endpoints in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:9-60`, routes use cases through `src/NetTrello.Api/Application/BoardService.cs:17-119`, and stores seeded data in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:12-32`. The client enters at `client/src/App.tsx:7-49`, centralizes board orchestration in `client/src/hooks/useBoard.ts:5-159`, and centralizes HTTP behavior in `client/src/api/boardClient.ts:24-112`. The strongest learning theme is boundary discipline: React types mirror API DTOs (`client/src/types.ts:1-65`, `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`), domain objects protect invariants (`src/NetTrello.Api/Domain/Board.cs:44-99`, `src/NetTrello.Api/Domain/Card.cs:48-62`), and persistence hides behind `IBoardRepository` (`src/NetTrello.Api/Application/IBoardRepository.cs:5-15`). The missing pieces are also useful: there are no test projects, EF Core migrations, auth policies, CI workflows, or background workers found in the current file inventory.

Senior mindset line: a junior asks "how do I make it work?", a mid-level engineer asks "is this the right pattern?", and a senior asks "what does this commit us to, who pays, and how do we reduce risk?"

## How to Use This Lab

| Track | Path |
| --- | --- |
| Weekend | Read `00-fast-track.md`, trace card movement, make one tiny UI or validation change, write a teach-back. |
| Two weeks | Add `01-codebase-cartography/`, `02-stack-and-language-mastery/`, and the debugging scenarios. |
| Eight weeks | Work through the tickets in `06-contribution-practice/`, adding tests before broadening features. |
| Ongoing | Use `08-reference/risk-register.md`, review katas, and senior projects during design reviews. |

Recommended routes:

- Brand-new junior: `00-fast-track.md` -> `01-codebase-cartography/02-file-reading-order.md` -> `04-code-reading-gym/01-annotation-drills.md`.
- Junior with .NET/React familiarity: `01-codebase-cartography/05-key-flows.md` -> `03-architecture-and-patterns/05-pattern-catalog.md` -> good-first tickets.
- Mid-level new to repo: `03-architecture-and-patterns/01-boundaries-and-layers.md` -> `05-quality-engineering/01-testing-strategy.md` -> mid-level tickets.
- Senior doing architecture review: `03-architecture-and-patterns/06-architecture-critique.md` -> `08-reference/risk-register.md` -> senior build projects.

## Conventions

- Codebase claims include exact path and line anchors like `src/NetTrello.Api/Program.cs:11-13`.
- Fake examples are labeled `Illustrative fake code: not from this repo`.
- Every drill should end with a self-grade: Basic means you followed the path, Solid means you explained the why, Strong means you predicted a failure mode.
- Verification notes appear in key docs and in `08-reference/verification-log.md`.

