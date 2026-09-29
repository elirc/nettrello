# Type System And Contracts

## C# Nullable Reference Types

Nullable references are enabled in `src/NetTrello.Api/NetTrello.Api.csproj:3-5`. This forces the code to model absent data explicitly, such as optional board descriptions in `src/NetTrello.Api/Contracts/BoardDtos.cs:32-32` and optional card fields in `src/NetTrello.Api/Contracts/BoardDtos.cs:36-50`.

Why it matters: a nullable type tells reviewers to ask "what does absent mean here?"

## Records As HTTP Contracts

DTOs are records in `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`. Records are good for immutable-ish data transfer shapes. They are not domain entities; domain state and behavior live in classes such as `src/NetTrello.Api/Domain/Board.cs:3-157`.

Failure mode: adding behavior to DTOs or returning domain classes directly couples HTTP clients to internal rules.

## C# Classes For Domain Entities

`Board`, `BoardColumn`, and `Card` are sealed classes with private setters and mutation methods (`src/NetTrello.Api/Domain/Board.cs:21-35`, `src/NetTrello.Api/Domain/BoardColumn.cs:23-31`, `src/NetTrello.Api/Domain/Card.cs:28-46`). This expresses ownership: outside code asks the object to change; it does not set arbitrary properties.

## TypeScript API Mirror

Client interfaces in `client/src/types.ts:1-65` mirror DTOs in `src/NetTrello.Api/Contracts/BoardDtos.cs:5-54`. There is no code generation, so contract drift is possible.

Failure mode: changing `CardPriority` in `src/NetTrello.Api/Domain/CardPriority.cs:1-8` without updating `client/src/types.ts:1-1` causes runtime mismatch despite TypeScript passing.

## Enum Strings

The API serializes enums as strings via `JsonStringEnumConverter` in `src/NetTrello.Api/Program.cs:14-17`. The client expects string union values in `client/src/types.ts:1-1`.

Drill: Add a fake enum value on paper. List all files you would inspect before shipping it.

