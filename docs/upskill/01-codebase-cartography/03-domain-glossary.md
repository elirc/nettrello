# Domain Glossary

| Term | Meaning in this repo | Evidence | Senior question |
| --- | --- | --- | --- |
| Board | Container for columns, cards, members, timestamps, and version | `src/NetTrello.Api/Domain/Board.cs:8-35` | Should version be per board or per aggregate part? |
| Column | Ordered lane with title, card limit, and cards | `src/NetTrello.Api/Domain/BoardColumn.cs:7-31` | What happens when limits are changed after cards exist? |
| Card | Work item with title, description, priority, assignee, due date, labels | `src/NetTrello.Api/Domain/Card.cs:5-46` | Which fields become searchable or indexed later? |
| Member | Board participant with id, display name, email | `src/NetTrello.Api/Domain/BoardMember.cs:3-19` | Is membership an auth boundary or just display data? |
| Priority | Low, Medium, High, Critical enum | `src/NetTrello.Api/Domain/CardPriority.cs:1-8`, `client/src/types.ts:1-1` | How do enum changes roll out across API/client? |
| Label | Normalized string attached to card | `src/NetTrello.Api/Domain/Card.cs:75-82` | Should labels become first-class records? |
| Position | Zero-based order in columns/cards | `src/NetTrello.Api/Domain/BoardColumn.cs:74-80`, `src/NetTrello.Api/Domain/Board.cs:144-150` | Will integer reindexing scale under high write concurrency? |
| Version | Board-level optimistic concurrency token | `src/NetTrello.Api/Domain/Board.cs:31-31`, `src/NetTrello.Api/Application/BoardService.cs:106-119` | What user experience should conflicts produce? |
| Archive | Delete-like removal from active board | `src/NetTrello.Api/Domain/Board.cs:95-99`, `src/NetTrello.Api/Endpoints/BoardEndpoints.cs:40-41` | Is archive reversible? Where is audit history? |

## Drill

Pick three glossary terms and write one invariant for each. Then find the exact method that enforces or fails to enforce that invariant.

