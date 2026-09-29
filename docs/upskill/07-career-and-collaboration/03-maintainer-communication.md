# Maintainer Communication

## Asking A Question

Weak: "How do I add auth?"

Better:

> I traced board mutations through `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:11-41` and found no authorization checks. I think the first step is a resource policy around `boardId`, not just checking assignee membership in `src/NetTrello.Api/Domain/Board.cs:113-123`. Does that fit the intended direction?

## Bug Report Template

```md
## Repro
1. ...

## Expected
...

## Actual
...

## Evidence
- Network response:
- Code anchors:
- Environment:
```

## Responding To Review

Good:

> Agreed. I moved the rule from the endpoint into the domain so it is enforced for all service calls. I added tests for the service conflict path around `src/NetTrello.Api/Application/BoardService.cs:106-119`.

