# Writing PRs And RFCs

## PR Template

```md
## What
- Changed ...

## Why
- Solves ...

## How Tested
- Verified ...
- Not verified ...

## Risk
- Contract changes:
- Data changes:
- Security:
- Rollback:

## Anchors
- `path:line-line`
```

## When To Write An RFC

Write an RFC for durable persistence, auth, generated clients, real-time collaboration, or archive restore. These affect boundaries shown in `src/NetTrello.Api/Application/IBoardRepository.cs:5-15`, `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41`, and `client/src/types.ts:1-65`.

## Commit Messages

Use imperative, scoped messages:

- `api: require board version on mutations`
- `client: show conflict refresh message`
- `docs: add move-card debugging scenario`

