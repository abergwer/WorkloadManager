# Docker Setup Guide

## Prerequisites

You need:
- **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)
- **Docker Compose** (usually included with Docker Desktop)

### Install Docker

- **Windows/Mac**: Download [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Linux**: Follow [Docker Engine installation](https://docs.docker.com/engine/install/)

Verify installation:
```bash
docker --version
docker-compose --version
```

## Quick Start

### 1. Start the Application with Docker

Navigate to the project root directory and run:

```bash
docker-compose up
```

This will:
- Build the Docker image
- Start PostgreSQL container
- Start the WorkloadManager application
- Run all tests during the build
- Apply database migrations automatically

**First time may take 2-3 minutes** (building and pulling images).

### 2. Access the Application

Once containers are running:

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger/index.html
- **PostgreSQL**: localhost:5432

### 3. Stop the Application

```bash
docker-compose down
```

To also remove volumes (clear database):
```bash
docker-compose down -v
```

## Services

### PostgreSQL (Database)
- **Container**: `workload-db`
- **Host**: `postgres` (internal to Docker network)
- **Port**: `5432`
- **Username**: `postgres`
- **Password**: `postgres`
- **Database**: `jobsdb`
- **Data Volume**: `postgres_data`

### WorkloadManager (Web App)
- **Container**: `workload-app`
- **Ports**: `5000` (HTTP), `5001` (HTTPS)
- **Environment**: Development mode by default
- **Health Check**: Runs every 30 seconds

## Common Commands

### Run in Detached Mode (Background)
```bash
docker-compose up -d
```

### View Logs
```bash
# All services
docker-compose logs

# Specific service
docker-compose logs workloadmanager
docker-compose logs postgres

# Follow logs in real-time
docker-compose logs -f workloadmanager
```

### Execute Commands in Container
```bash
# Access the app container
docker-compose exec workloadmanager bash

# Run dotnet commands
docker-compose exec workloadmanager dotnet test

# Access PostgreSQL CLI
docker-compose exec postgres psql -U postgres -d jobsdb
```

### Rebuild Image
```bash
docker-compose up --build
```

To rebuild without cache:
```bash
docker-compose up --build --no-cache
```

### Remove All Containers and Images
```bash
docker-compose down -v
docker image rm workloadmanager_workloadmanager
```

## Environment Configuration

### Development vs Production

The docker-compose file is set to **Development** mode. To switch to Production:

Edit `docker-compose.yml`:
```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Production  # Change this
```

### Custom Environment Variables

Create a `.env` file in the project root:

```env
POSTGRES_PASSWORD=your_password
POSTGRES_DB=custom_dbname
ASPNETCORE_ENVIRONMENT=Development
```

Then reference in `docker-compose.yml`:
```yaml
environment:
  POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
  POSTGRES_DB: ${POSTGRES_DB}
```

## Troubleshooting

### Port Already in Use

**Error**: `bind: address already in use`

**Solution**: Change ports in `docker-compose.yml`:
```yaml
ports:
  - "5002:80"      # Use 5002 instead of 5000
  - "5003:443"     # Use 5003 instead of 5001
```

### Container Won't Start

**Check logs**:
```bash
docker-compose logs workloadmanager
```

Common issues:
- Database not ready: Check `postgres` service health
- Port conflicts: See "Port Already in Use" section
- Old containers running: Run `docker-compose down` first

### Database Connection Error

**Error**: `Host or service not known`

**Solution**: Ensure you're using `postgres` (service name) not `localhost` in connection strings.

The `docker-compose.yml` already handles this correctly with:
```
Host=postgres;Port=5432;...
```

### Tests Failing in Docker Build

**Error**: Tests fail during `docker build`

**Solution**: The Dockerfile runs tests. If they fail, the image won't build. Either:
1. Fix the failing tests locally first: `dotnet test`
2. Comment out the test line in Dockerfile temporarily

## Production Deployment

For production, you'd typically:

1. **Use a proper PostgreSQL image** with persistent storage
2. **Set stronger passwords** via environment variables
3. **Use HTTPS certificates** (not self-signed)
4. **Enable proper logging** and monitoring
5. **Set resource limits** for containers

Example for production `docker-compose.yml`:
```yaml
services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}  # From secret/env file
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: always

  workloadmanager:
    image: your-registry/workloadmanager:latest
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__Postgres: ${DATABASE_URL}
    ports:
      - "80:80"
      - "443:443"
    restart: always
```

## Docker Compose Reference

| Command | Purpose |
|---------|---------|
| `docker-compose up` | Start and attach to services |
| `docker-compose up -d` | Start services in background |
| `docker-compose down` | Stop and remove containers |
| `docker-compose down -v` | Stop and remove containers + volumes |
| `docker-compose logs [service]` | View logs |
| `docker-compose logs -f [service]` | Follow logs in real-time |
| `docker-compose exec [service] [cmd]` | Execute command in container |
| `docker-compose ps` | List running containers |
| `docker-compose build` | Build images |
| `docker-compose pull` | Pull latest images |

## Volumes

The `postgres_data` volume persists your database. It's stored in Docker's managed directory.

To inspect:
```bash
docker volume inspect postgres_data
```

To back up database:
```bash
docker-compose exec postgres pg_dump -U postgres jobsdb > backup.sql
```

To restore:
```bash
docker-compose exec -T postgres psql -U postgres jobsdb < backup.sql
```

## Next Steps

1. ✅ Install Docker
2. ✅ Run `docker-compose up`
3. ✅ Visit http://localhost:5000/swagger
4. ✅ Test the API
5. ✅ Run `docker-compose down` when done

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres/)
