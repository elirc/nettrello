# .NET Runtime Model

## CLR, JIT, And Process Shape

The API is an ASP.NET Core process built from `WebApplication.CreateBuilder` and `app.Run()` in `src/NetTrello.Api/Program.cs:7-39`. The CLR loads the compiled assembly, JIT-compiles hot methods, and keeps singleton services alive for the process lifetime. That matters because `InMemoryBoardRepository` is registered singleton in `src/NetTrello.Api/Program.cs:11-13`, so its `ConcurrentDictionary` in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:9-10` is process-local state, not durable storage.

Failure mode: assuming singleton memory is a database. Restarting the process loses boards created after startup, because only constructor seed data is re-created at `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:12-32`.

Drill: Explain why two Docker containers would not share boards even though both run the same image from `src/NetTrello.Api/Dockerfile:10-16`.

## GC And Allocations

The repository clones boards on read and write (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:38-45`, `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:77-79`). The clone methods allocate new aggregate objects (`src/NetTrello.Api/Domain/Board.cs:102-110`, `src/NetTrello.Api/Domain/BoardColumn.cs:69-72`, `src/NetTrello.Api/Domain/Card.cs:65-73`). This is a reasonable teaching tradeoff: isolation is clear, but large boards would create allocation pressure.

Failure mode: optimizing away clones without adding another isolation strategy can let callers mutate stored state without repository control.

Drill: Estimate allocations for a board with 10 columns and 1,000 cards when calling `GetAsync`.

## Async, Task, And CancellationToken

Endpoints accept `CancellationToken` and pass it to services in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:13-41`. The service passes tokens to repository calls in `src/NetTrello.Api/Application/BoardService.cs:17-43` and `src/NetTrello.Api/Application/BoardService.cs:106-119`. The repository checks cancellation before reads (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:35-46`) and uses `WaitAsync(cancellationToken)` for writes (`src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:49-69`).

Why it matters: cancellation lets ASP.NET stop wasted work when the client disconnects. In a future database implementation, cancellation should flow into EF Core `ToListAsync(cancellationToken)` or Dapper calls.

Failure mode: sync-over-async is not present here, but would look like `.Result` or `.Wait()` inside an endpoint or service. Search before adding it.

## Task vs ValueTask

This repo uses `Task<T>` for async APIs (`src/NetTrello.Api/Application/IBoardRepository.cs:5-15`). That is the right default for app code. `ValueTask<T>` is useful only when profiling shows many synchronous completions on a hot path and the team understands its consumption rules.

Drill: Do not change `Task<T>` to `ValueTask<T>` until you can name the measured allocation problem.

## IDisposable And Lifetime

`SemaphoreSlim` in `src/NetTrello.Api/Infrastructure/InMemoryBoardRepository.cs:10-10` implements `IDisposable`, but the singleton repository does not dispose it. In this small process-lifetime service this is low risk, but a senior would note it in a review if the object became short-lived.

Drill: If `InMemoryBoardRepository` became scoped, what lifetime or disposal problem would you introduce?

