FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/NetTrello.Api/NetTrello.Api.csproj", "src/NetTrello.Api/"]
RUN dotnet restore "src/NetTrello.Api/NetTrello.Api.csproj"

COPY . .
RUN dotnet publish "src/NetTrello.Api/NetTrello.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetTrello.Api.dll"]
