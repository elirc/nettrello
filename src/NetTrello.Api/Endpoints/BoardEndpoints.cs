using NetTrello.Api.Application;
using NetTrello.Api.Contracts;
using NetTrello.Api.Domain;

namespace NetTrello.Api.Endpoints;

public static class BoardEndpoints
{
    public static IEndpointRouteBuilder MapBoardEndpoints(this IEndpointRouteBuilder app)
    {
        var boards = app.MapGroup("/api/boards").WithTags("Boards");

        boards.MapGet("/", async (BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ListBoardsAsync(cancellationToken)));

        boards.MapPost("/", async (CreateBoardRequest request, BoardService service, CancellationToken cancellationToken) =>
        {
            var created = await service.CreateBoardAsync(request, cancellationToken);
            return Results.Created($"/api/boards/{created.Id}", created);
        });

        boards.MapGet("/{boardId:guid}", async (Guid boardId, BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetBoardAsync(boardId, cancellationToken)));

        boards.MapPost("/{boardId:guid}/columns", async (Guid boardId, CreateColumnRequest request, HttpRequest httpRequest, BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.AddColumnAsync(boardId, request, ReadExpectedVersion(httpRequest), cancellationToken)));

        boards.MapPut("/{boardId:guid}/columns/{columnId:guid}/move", async (Guid boardId, Guid columnId, MoveColumnRequest request, HttpRequest httpRequest, BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.MoveColumnAsync(boardId, columnId, request, ReadExpectedVersion(httpRequest), cancellationToken)));

        boards.MapPost("/{boardId:guid}/columns/{columnId:guid}/cards", async (Guid boardId, Guid columnId, CreateCardRequest request, HttpRequest httpRequest, BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.AddCardAsync(boardId, columnId, request, ReadExpectedVersion(httpRequest), cancellationToken)));

        boards.MapPatch("/{boardId:guid}/cards/{cardId:guid}", async (Guid boardId, Guid cardId, UpdateCardRequest request, HttpRequest httpRequest, BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateCardAsync(boardId, cardId, request, ReadExpectedVersion(httpRequest), cancellationToken)));

        boards.MapPut("/{boardId:guid}/cards/{cardId:guid}/move", async (Guid boardId, Guid cardId, MoveCardRequest request, HttpRequest httpRequest, BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.MoveCardAsync(boardId, cardId, request, ReadExpectedVersion(httpRequest), cancellationToken)));

        boards.MapDelete("/{boardId:guid}/cards/{cardId:guid}", async (Guid boardId, Guid cardId, HttpRequest httpRequest, BoardService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.ArchiveCardAsync(boardId, cardId, ReadExpectedVersion(httpRequest), cancellationToken)));

        return app;
    }

    private static int? ReadExpectedVersion(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("X-Board-Version", out var value))
        {
            return null;
        }

        if (!int.TryParse(value, out var version))
        {
            throw DomainException.BadRequest("board.version_invalid", "X-Board-Version must be an integer.");
        }

        return version;
    }
}
