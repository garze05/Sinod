using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sinod.Api.Data;
using Sinod.Api.Endpoints;
using Sinod.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
});

var connectionString = builder.Configuration.GetConnectionString("Sinod");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Configure ConnectionStrings.Sinod. For local development, run bash scripts/setup-local.sh from root.");

builder.Services.AddDbContext<SinodDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<CommercialService>();
builder.Services.AddHealthChecks().AddDbContextCheck<SinodDbContext>("postgresql");

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.MapHealthEndpoints();
app.MapClientEndpoints();
app.MapCatalogEndpoints();
app.MapCommercialEndpoints();
app.Run();

public partial class Program;
