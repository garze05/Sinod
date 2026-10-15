using Microsoft.EntityFrameworkCore;
using Sinod.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Sinod");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Configure ConnectionStrings.Sinod. For local development, run bash scripts/setup-local.sh from root.");
}

// Add PostgreSQL Database context
builder.Services.AddDbContext<SinodDbContext>(options => options.UseNpgsql(connectionString));

// Add Native ASP.NET Health Service
builder.Services.AddHealthChecks().AddDbContextCheck<SinodDbContext>("postgresql");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
