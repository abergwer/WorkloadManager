# First stage: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["WorkloadManager/WorkloadManager.csproj", "WorkloadManager/"]
COPY ["WorkloadManager.Tests/WorkloadManager.Tests.csproj", "WorkloadManager.Tests/"]

# Restore dependencies
RUN dotnet restore "WorkloadManager/WorkloadManager.csproj"
RUN dotnet restore "WorkloadManager.Tests/WorkloadManager.Tests.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR /src/WorkloadManager
RUN dotnet build "WorkloadManager.csproj" -c Release -o /app/build

# Run tests
WORKDIR /src/WorkloadManager.Tests
RUN dotnet test

# Publish
WORKDIR /src/WorkloadManager
RUN dotnet publish "WorkloadManager.csproj" -c Release -o /app/publish

# Second stage: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application from build stage
COPY --from=build /app/publish .

# Expose ports
EXPOSE 80 443

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "WorkloadManager.dll"]
