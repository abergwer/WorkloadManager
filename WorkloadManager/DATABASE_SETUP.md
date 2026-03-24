# Database Setup and Migrations

## Overview

The WorkloadManager application uses **Entity Framework Core** with **PostgreSQL** for data persistence.

## Prerequisites

1. PostgreSQL database server running on `localhost:5432`
2. Database `jobsdb` created (or adjust connection string in `appsettings.json`)
3. User `postgres` with password (configured in `appsettings.json`)

## Connection String

Located in `appsettings.json`:

```json
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5432;Database=jobsdb;Username=postgres;Password=asher1133"
}
```

Adjust as needed for your PostgreSQL setup.

## Initial Setup

### 1. Install Entity Framework Core CLI Tools

```powershell
dotnet tool install --global dotnet-ef
```

Or update if already installed:

```powershell
dotnet tool update --global dotnet-ef
```

### 2. Create Initial Migration

Run this in the project root directory:

```powershell
dotnet ef migrations add InitialCreate
```

This creates a migration file in the `Migrations` folder.

### 3. Apply Migration to Database

```powershell
dotnet ef database update
```

This creates the database schema with all tables and indexes.

## Subsequent Migrations

When you modify the data models:

### 1. Create a new migration

```powershell
dotnet ef migrations add DescriptionOfChanges
```

### 2. Apply the migration

```powershell
dotnet ef database update
```

## Development Workflow

The application automatically applies migrations on startup in development mode (see `Program.cs`):

```csharp
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkloadDbContext>();
        dbContext.Database.Migrate();
    }
}
```

## Database Schema

### Jobs Table
- `Id` (int, PK)
- `Type` (string)
- `Name` (string)
- `Description` (string)
- `Status` (int enum)
- `Priority` (int)
- `CurrentAttempt` (int)
- `MaxAttempts` (int)
- `ProgressPercentage` (int)
- `Payload` (jsonb)
- `Result` (jsonb)
- `ErrorMessage` (string)
- `ErrorDetails` (text)
- `IdempotencyKey` (string, unique)
- `ScheduledFor` (datetime)
- `CreatedAt` (datetime)
- `StartedAt` (datetime)
- `CompletedAt` (datetime)
- `UpdatedAt` (datetime)

**Indexes:**
- Status
- Type
- CreatedAt
- IdempotencyKey (unique)

### JobLogs Table
- `Id` (int, PK)
- `JobId` (int, FK)
- `Level` (int enum)
- `Message` (string)
- `Metadata` (jsonb)
- `Timestamp` (datetime)

**Indexes:**
- JobId
- Timestamp

**Foreign Keys:**
- JobId → Jobs.Id (cascade delete)

## Common EF Core Commands

### View pending migrations

```powershell
dotnet ef migrations list
```

### Remove last migration (if not applied to database)

```powershell
dotnet ef migrations remove
```

### Generate SQL script for migration

```powershell
dotnet ef migrations script
```

### Drop database

```powershell
dotnet ef database drop
```

## Troubleshooting

### "Pending migrations" error

Application won't run if there are pending migrations. Either:
- Apply migrations: `dotnet ef database update`
- Or remove migrations: `dotnet ef migrations remove`

### Connection refused error

- Verify PostgreSQL is running
- Check connection string in `appsettings.json`
- Verify database exists: `createdb jobsdb`

### Migration conflicts

If migrations conflict with existing schema:

1. Delete the conflicting migration file
2. Run `dotnet ef database drop` (be careful - loses data!)
3. Recreate and apply fresh migrations

## Best Practices

1. **Always commit migrations to version control**
2. **Test migrations on a copy of production data first**
3. **Use meaningful migration names**: `dotnet ef migrations add AddUserJobTypesSupport`
4. **Review generated SQL before applying to production**
5. **Keep migration scripts as part of your deployment process**

## Running Tests with Database

For integration tests, you can:

1. Use an in-memory database: `.UseInMemoryDatabase("test")`
2. Use a test database with a different connection string
3. Use database snapshots/transactions for test isolation

Example for testing:

```csharp
var options = new DbContextOptionsBuilder<WorkloadDbContext>()
    .UseInMemoryDatabase("TestJobDb")
    .Options;

using (var context = new WorkloadDbContext(options))
{
    context.Database.EnsureCreated();
    // Your tests here
}
```
