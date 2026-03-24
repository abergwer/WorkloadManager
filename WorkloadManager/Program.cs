using Microsoft.EntityFrameworkCore;
using WorkloadManager.Database;
using WorkloadManager.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres");

// Add Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<WorkloadDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add repository
builder.Services.AddScoped<WorkloadRepository>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<JobService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations automatically in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkloadDbContext>();
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


app.UseAuthorization();

app.MapControllers();

app.Run();