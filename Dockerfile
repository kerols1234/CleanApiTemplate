# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-level build files first for better layer caching on restore.
COPY ["global.json", "nuget.config", "Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["CleanApi.slnx", "./"]

# Copy project files, then restore (cached unless a .csproj changes).
COPY ["src/CleanApi.Domain/CleanApi.Domain.csproj", "src/CleanApi.Domain/"]
COPY ["src/CleanApi.Application/CleanApi.Application.csproj", "src/CleanApi.Application/"]
COPY ["src/CleanApi.Infrastructure/CleanApi.Infrastructure.csproj", "src/CleanApi.Infrastructure/"]
COPY ["src/CleanApi.Api/CleanApi.Api.csproj", "src/CleanApi.Api/"]
RUN dotnet restore "src/CleanApi.Api/CleanApi.Api.csproj"

# Copy the rest and publish.
COPY . .
RUN dotnet publish "src/CleanApi.Api/CleanApi.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
# Chiseled (distroless) ASP.NET image: minimal surface, runs as non-root ($APP_UID) by default.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "CleanApi.Api.dll"]
