using System.Text.Json;
using NetTrello.Api.Application;
using NetTrello.Api.Contracts;
using NetTrello.Api.Domain;
using NetTrello.Api.Infrastructure;

var now = new DateTimeOffset(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);
var clock = new FixedClock(now);
var repository = new InMemoryBoardRepository();
var service = new BoardService(repository, clock);
var board = new Board(Guid.NewGuid(), "Checks", "Regression board", now);
var source = board.AddColumn("Source", 0, now);
var target = board.AddColumn("Target", 2, now);
var member = new BoardMember(Guid.NewGuid(), "Test Member", "test@example.com");
var memberBoard = new Board(board.Id, board.Name, board.Description, now,
    columns: new[] { source, target }, members: new[] { member });
var first = memberBoard.AddCard(source.Id, "First", "Keep me", CardPriority.High, member.Id, now.AddDays(1), ["one", "two"], now);
var second = memberBoard.AddCard(source.Id, "Second", "Keep too", CardPriority.Medium, null, null, ["two"], now);
var targetOne = memberBoard.AddCard(target.Id, "Target one", null, CardPriority.Low, null, null, [], now);
var targetTwo = memberBoard.AddCard(target.Id, "Target two", null, CardPriority.Low, null, null, [], now);
await repository.AddAsync(memberBoard, CancellationToken.None);

var beforeMove = Describe(await repository.GetAsync(board.Id, CancellationToken.None));
ExpectDomain(() => memberBoard.MoveCard(first.Id, Guid.NewGuid(), 0, now), "column.not_found");
ExpectEqual(beforeMove, Describe(memberBoard), "missing target must not remove the source card");
ExpectDomain(() => memberBoard.MoveCard(first.Id, target.Id, 0, now), "column.card_limit");
ExpectEqual(beforeMove, Describe(memberBoard), "full target must not remove the source card");
ExpectDomain(() => memberBoard.MoveCard(first.Id, source.Id, -1, now), "validation.non_negative");
ExpectEqual(beforeMove, Describe(memberBoard), "negative position must not mutate the source");

memberBoard.MoveCard(second.Id, source.Id, 0, now.AddMinutes(1));
ExpectEqual("Second,First", string.Join(',', memberBoard.Columns[0].Cards.Select(card => card.Title)), "same-column reorder works at capacity elsewhere");

var cardBeforeInvalid = DescribeCard(first);
ExpectDomain(() => first.Update("Changed", "Changed description", CardPriority.Critical, member.Id, now.AddDays(3), ["ok", ""], now.AddMinutes(2)), "validation.required");
ExpectEqual(cardBeforeInvalid, DescribeCard(first), "late label validation preserves every card field");

var titleOnly = Request("{\"title\":\"Updated title\"}");
var updated = await service.UpdateCardAsync(board.Id, first.Id, titleOnly, null, CancellationToken.None);
var updatedCard = updated.Columns.SelectMany(column => column.Cards).Single(card => card.Id == first.Id);
ExpectEqual("Updated title", updatedCard.Title, "title-only PATCH applies title");
ExpectEqual("Keep me", updatedCard.Description, "title-only PATCH preserves description");
ExpectEqual("High", updatedCard.Priority.ToString(), "title-only PATCH preserves priority");
ExpectEqual(2, updatedCard.Labels.Count, "title-only PATCH preserves labels");

var clears = Request("{\"description\":null,\"assigneeId\":null,\"dueDate\":null,\"labels\":null,\"priority\":\"Low\"}");
updated = await service.UpdateCardAsync(board.Id, first.Id, clears, null, CancellationToken.None);
updatedCard = updated.Columns.SelectMany(column => column.Cards).Single(card => card.Id == first.Id);
ExpectEqual(string.Empty, updatedCard.Description, "explicit null clears description");
ExpectEqual(null, updatedCard.AssigneeId, "explicit null clears assignee");
ExpectEqual(null, updatedCard.DueDate, "explicit null clears due date");
ExpectEqual(0, updatedCard.Labels.Count, "explicit null clears labels");
ExpectEqual("Low", updatedCard.Priority.ToString(), "priority string is parsed");

var invalidBefore = Describe(await repository.GetAsync(board.Id, CancellationToken.None));
await ExpectDomainAsync(() => service.UpdateCardAsync(board.Id, first.Id, Request("{\"priority\":\"Unknown\"}"), null, CancellationToken.None), "card.priority_invalid");
ExpectEqual(invalidBefore, Describe(await repository.GetAsync(board.Id, CancellationToken.None)), "invalid enum rejects before mutation");
await ExpectDomainAsync(() => service.UpdateCardAsync(board.Id, first.Id, Request("{\"title\":null}"), null, CancellationToken.None), "card.field_invalid");
await ExpectDomainAsync(() => service.UpdateCardAsync(board.Id, first.Id, Request("{\"dueDate\":42}"), null, CancellationToken.None), "card.field_invalid");
ExpectJson(() => Request("{\"unknownField\":true}"), "unknown PATCH properties are rejected");
await ExpectDomainAsync(() => service.UpdateCardAsync(board.Id, first.Id, Request("{}"), null, CancellationToken.None), "card.patch_empty");

var fullUpdate = new UpdateCardRequest {
    Title = Element("Full update"), Description = Element("All fields"), Priority = Element("Critical"),
    AssigneeId = Element(member.Id.ToString()), DueDate = Element(now.AddDays(5).ToString("O")), Labels = ElementArray("alpha", "beta")
};
updated = await service.UpdateCardAsync(board.Id, first.Id, fullUpdate, null, CancellationToken.None);
updatedCard = updated.Columns.SelectMany(column => column.Cards).Single(card => card.Id == first.Id);
ExpectEqual("Full update", updatedCard.Title, "complete UI update remains compatible");
ExpectEqual(2, updatedCard.Labels.Count, "complete UI update retains labels");

var version = updated.Version;
await ExpectDomainAsync(() => service.AddColumnAsync(board.Id, new CreateColumnRequest("stale"), version - 1, CancellationToken.None), "board.version_conflict");
var competing = await Task.WhenAll(
    Capture(() => service.AddColumnAsync(board.Id, new CreateColumnRequest("one"), version, CancellationToken.None)),
    Capture(() => service.AddColumnAsync(board.Id, new CreateColumnRequest("two"), version, CancellationToken.None)));
ExpectEqual(1, competing.Count(result => result), "one competing expected-version write succeeds");
ExpectEqual(1, competing.Count(result => !result), "one competing expected-version write conflicts");

var clone = await repository.GetAsync(board.Id, CancellationToken.None) ?? throw new Exception("board missing");
clone.Rename("mutated clone", "should not persist", now.AddMinutes(3));
var reread = await repository.GetAsync(board.Id, CancellationToken.None) ?? throw new Exception("board missing");
ExpectEqual("Checks", reread.Name, "repository returns an isolated clone");

Console.WriteLine("NetTrello checks passed: domain validation, PATCH presence, version conflicts, clone isolation.");

static JsonElement Element(string value) => JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement.Clone();
static JsonElement ElementArray(params string[] values) => JsonDocument.Parse(JsonSerializer.Serialize(values)).RootElement.Clone();
static UpdateCardRequest Request(string json) => JsonSerializer.Deserialize<UpdateCardRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? throw new Exception("request did not deserialize");
static void ExpectJson(Action action, string message)
{
    try { action(); throw new Exception($"Expected JSON rejection: {message}"); }
    catch (JsonException) { }
}
static string Describe(Board? board) => board is null ? "<null>" : JsonSerializer.Serialize(board.Columns.Select(column => new {
    column.Id, column.Title, column.CardLimit,
    Cards = column.Cards.Select(card => new { card.Id, card.Title, card.Description, card.Priority, card.AssigneeId, card.DueDate, card.Labels, card.Position, card.UpdatedAt })
}));
static string DescribeCard(Card card) => JsonSerializer.Serialize(new { card.Id, card.Title, card.Description, card.Priority, card.AssigneeId, card.DueDate, card.Labels, card.Position, card.UpdatedAt });
static void ExpectEqual<T>(T expected, T actual, string message)
{
    if (!Equals(expected, actual))
    {
        throw new Exception($"{message}: expected {expected}, actual {actual}");
    }
}
static void ExpectDomain(Action action, string code) {
    try { action(); throw new Exception($"Expected {code}."); }
    catch (DomainException error) { ExpectEqual(code, error.Code, "domain error code"); }
}
static async Task ExpectDomainAsync(Func<Task> action, string code) {
    try { await action(); throw new Exception($"Expected {code}."); }
    catch (DomainException error) { ExpectEqual(code, error.Code, "domain error code"); }
}
static async Task<bool> Capture(Func<Task> action) {
    try { await action(); return true; }
    catch (DomainException) { return false; }
}

sealed class FixedClock(DateTimeOffset value) : IClock
{
    public DateTimeOffset UtcNow => value;
}
