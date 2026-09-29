using NetTrello.Api.Application;
using NetTrello.Api.Endpoints;
using NetTrello.Api.Infrastructure;
using NetTrello.Api.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IBoardRepository, InMemoryBoardRepository>();
builder.Services.AddScoped<BoardService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("Client");

app.MapHealthChecks("/health");
app.MapBoardEndpoints();

app.Run();

public partial class Program
{
}
