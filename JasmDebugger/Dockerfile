# Dockerfile for JasmDebugger service
# Multi-stage build for .NET 10

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 4000
EXPOSE 5076

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["JasmDebugger/JasmDebugger.csproj", "JasmDebugger/"]
COPY ["JasmDebugger.Client/JasmDebugger.Client.csproj", "JasmDebugger.Client/"]
RUN dotnet restore "JasmDebugger/JasmDebugger.csproj"
COPY . .
WORKDIR "/src/JasmDebugger"
RUN dotnet build "JasmDebugger.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "JasmDebugger.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure ASP.NET listens on port 5076
ENV ASPNETCORE_HTTP_PORTS=5076
ENV ASPNETCORE_URLS=http://+:5076

ENTRYPOINT ["dotnet", "JasmDebugger.dll"]
