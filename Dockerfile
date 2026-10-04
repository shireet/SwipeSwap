FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Directory.Packages.props .
COPY ["src/SwipeSwap.WebAPI/SwipeSwap.WebAPI.csproj", "src/SwipeSwap.WebAPI/"]
COPY ["src/SwipeSwap.Infrastructure.Jwt/SwipeSwap.Infrastructure.Jwt.csproj", "src/SwipeSwap.Infrastructure.Jwt/"]
COPY ["src/SwipeSwap.Infrastructure.Postgres/SwipeSwap.Infrastructure.Postgres.csproj", "src/SwipeSwap.Infrastructure.Postgres/"]
COPY ["src/SwipeSwap.Infrastructure.Redis/SwipeSwap.Infrastructure.Redis.csproj", "src/SwipeSwap.Infrastructure.Redis/"]
COPY ["src/SwipeSwap.Domain/SwipeSwap.Domain.csproj", "src/SwipeSwap.Domain/"]
COPY ["src/SwipeSwap.EntryPoint/SwipeSwap.EntryPoint.csproj", "src/SwipeSwap.EntryPoint/"]
COPY ["src/SwipeSwap.Application/SwipeSwap.Application.csproj", "src/SwipeSwap.Application/"]

RUN dotnet restore "src/SwipeSwap.WebAPI/SwipeSwap.WebAPI.csproj"

COPY src/ src/

WORKDIR /src/src/SwipeSwap.WebAPI
RUN dotnet publish "SwipeSwap.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SwipeSwap.WebApi.dll"]
