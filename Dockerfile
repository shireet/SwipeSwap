FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/SwipeSwap.WebApi/SwipeSwap.WebApi.csproj", "SwipeSwap.WebApi/"]
COPY ["src/SwipeSwap.Infrastructure/SwipeSwap.Infrastructure.csproj", "SwipeSwap.Infrastructure/"]
COPY ["src/SwipeSwap.Domain/SwipeSwap.Domain.csproj", "SwipeSwap.Domain/"]
COPY ["src/SwipeSwap.EntryPoint/SwipeSwap.EntryPoint.csproj", "SwipeSwap.EntryPoint/"]
COPY ["src/SwipeSwap.Application/SwipeSwap.Application.csproj", "SwipeSwap.Application/"]

RUN dotnet restore "SwipeSwap.WebApi/SwipeSwap.WebApi.csproj"

COPY src/ .

WORKDIR "/src/SwipeSwap.WebApi"
RUN dotnet build "SwipeSwap.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SwipeSwap.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SwipeSwap.WebApi.dll"]