# Security Checklist

## Pre-Merge Checklist

- Authn/authz: Are endpoints protected? Current endpoints in `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` are not.
- IDOR: Can caller mutate another board by guessing `boardId`? Current routes bind ids at `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:22-41` with no caller check.
- Input validation: Domain guards exist at `src/NetTrello.Api/Domain/Guard.cs:3-34`; ensure new fields use them or equivalent.
- XSS: React escapes text by default where titles/descriptions render in `client/src/components/KanbanCard.tsx:29-42`; be careful before adding raw HTML.
- CSRF: API appears tokenless and non-cookie-based today; reassess if cookies are added.
- CORS: Config exists at `src/NetTrello.Api/appsettings.json:1-8`; remember it is browser policy, not authorization.
- SQL injection: No SQL currently. If EF/Dapper is added, parameterize.
- Open redirects: No redirects found.
- Secrets: No secret loader found; do not commit secrets in appsettings.
- Dependency risk: npm dependencies in `client/package.json:12-18` lack a lockfile.
- File uploads: None found.
- Webhooks: None found; future webhooks need signature validation and idempotency.
- Rate limiting: None found; add before public write API.
- Security headers: Basic headers exist at `src/NetTrello.Api/Middleware/SecurityHeadersMiddleware.cs:12-20`; CSP absent.

## Drill

Write a threat model for `DELETE /api/boards/{boardId}/cards/{cardId}` anchored to `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:40-41`.

