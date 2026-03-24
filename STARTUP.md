# WorkloadManager - Startup Guide

## Prerequisites

- **.NET 8.0** or later
- **PostgreSQL 12+** database server
- **Entity Framework Core CLI** tools (optional, for migrations)

### Install .NET SDK

If you don't have .NET 8.0, download it from [dotnet.microsoft.com](https://dotnet.microsoft.com/download).

### Install PostgreSQL

Download and install PostgreSQL from [postgresql.org](https://www.postgresql.org/download/).

For local development, the default connection string assumes:
- **Host**: `localhost`
- **Port**: `5432`
- **Username**: `postgres`
- **Password**: Set during installation

## Initial Setup

### 1. Create the Database

```sql
CREATE DATABASE jobsdb;
```

Or connect to PostgreSQL and run this SQL:
```sql
CREATE DATABASE jobsdb OWNER postgres;
```

### 2. Configure Connection String

The connection string is located in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=jobsdb;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

**Important**: Replace `YOUR_PASSWORD` with your PostgreSQL password.

### 3. Run Database Migrations

The application automatically applies migrations on startup in development mode. Simply run the application and the database schema will be created automatically.

Alternatively, manually apply migrations using Entity Framework CLI:

```bash
# Install EF CLI (one-time)
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update
```

## Running the Application

### Development Mode

Navigate to the project directory and run:

```bash
cd WorkloadManager
dotnet run
```

The application will start on:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

### Production Mode

```bash
dotnet publish -c Release
# Run the published executable
```

## API Documentation

Once the application is running, visit:
- **Swagger UI**: `https://localhost:5001/swagger/index.html`

This provides an interactive API documentation where you can:
- View all available endpoints
- Test API calls directly
- View request/response schemas

## Key Endpoints

- **Create Job**: `POST /api/jobs/create`
- **Get Job**: `GET /api/jobs/{id}`
- **Get Job Types**: `GET /api/jobs/types`
- **Execute Job**: `POST /api/jobs/execute/{jobType}`

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test WorkloadManager.Tests
```

### Run Specific Test Class

```bash
dotnet test --filter ClassName=JobServiceTests
```

## Troubleshooting

### Connection Refused Error

**Error**: `Npgsql.NpgsqlException: Unable to connect to the server: Cannot assign a requested address`

**Solution**:
1. Verify PostgreSQL is running
2. Check the connection string in `appsettings.json`
3. Ensure the database exists: `CREATE DATABASE jobsdb;`

### Migration Issues

**Error**: `The type or namespace name 'Jobs' could not be found`

**Solution**:
```bash
# Reset and reapply migrations
dotnet ef database drop
dotnet ef database update
```

### Port Already in Use

If port 5001 is already in use, configure a different port in `launchSettings.json`:

```json
"https": {
  "CommandName": "Project",
  "launchBrowser": true,
  "launchUrl": "swagger",
  "applicationUrl": "https://localhost:5002;http://localhost:5000"
}
```

## Environment Variables

Set environment variables for different configurations:

```bash
# Development
set ASPNETCORE_ENVIRONMENT=Development

# Production
set ASPNETCORE_ENVIRONMENT=Production
```

## Project Structure

```
WorkloadManager/
├── Controllers/          # API endpoints
├── Database/            # EF Core DbContext & Repository
├── Models/              # Data models
├── Services/            # Business logic
├── Properties/          # Launch settings
├── appsettings.json     # Configuration
└── Program.cs           # Application startup
```

## Next Steps

1. ✅ Install prerequisites
2. ✅ Create PostgreSQL database
3. ✅ Update connection string in `appsettings.json`
4. ✅ Run `dotnet run`
5. ✅ Visit `https://localhost:5001/swagger` to test API
6. ✅ Run `dotnet test` to verify all tests pass

## Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Web API Docs](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
