# Runtime base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# Restore
RUN dotnet restore "API/API.csproj"

# Copy everything
COPY . .

# Build
WORKDIR "/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Port configuration
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "API.dll"]
