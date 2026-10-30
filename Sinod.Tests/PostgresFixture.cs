using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Sinod.Api.Data;
using Testcontainers.PostgreSql;

namespace Sinod.Tests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly string database = $"sinod_test_{Guid.NewGuid():N}";
    private string? adminConnection;
    private PostgreSqlContainer? container;
    private bool databaseCreated;
    public ApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        adminConnection = Environment.GetEnvironmentVariable("SINOD_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(adminConnection))
        {
            container = new PostgreSqlBuilder("postgres:18")
                .WithDatabase("postgres")
                .WithPassword(Guid.NewGuid().ToString("N"))
                .Build();
            await container.StartAsync();
            adminConnection = container.GetConnectionString();
        }
        await using (var connection = new NpgsqlConnection(adminConnection))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE {database}", connection);
            await command.ExecuteNonQueryAsync();
            databaseCreated = true;
        }
        var testConnection = new NpgsqlConnectionStringBuilder(adminConnection) { Database = database }.ConnectionString;
        await using var db = new SinodDbContext(new DbContextOptionsBuilder<SinodDbContext>().UseNpgsql(testConnection).Options);
        await db.GetService<IMigrator>().MigrateAsync("20261017073905_InitialCreate");
        // Exercise an upgrade with two old versions belonging to the same opportunity.
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO "OdClients" ("Id", "FirstName", "Phone") VALUES (-1, 'Legacy client', '00000000');
            INSERT INTO "Opportunities" ("Id", "ClientId", "CreatedAt", "UpdatedAt", "Status", "EventDate")
                VALUES (-1, -1, NOW(), NOW(), 0, NOW());
            INSERT INTO "Quotations" ("Id", "OpportunityId", "State") VALUES (-2, -1, 0), (-1, -1, 0);
            """);
        await db.Database.MigrateAsync();
        Factory = new ApiFactory(testConnection);
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (Factory is not null) await Factory.DisposeAsync();
        }
        finally
        {
            try
            {
                if (databaseCreated)
                {
                    await using var connection = new NpgsqlConnection(adminConnection);
                    await connection.OpenAsync();
                    await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS {database} WITH (FORCE)", connection);
                    await command.ExecuteNonQueryAsync();
                    databaseCreated = false;
                }
            }
            finally
            {
                if (container is not null) await container.DisposeAsync();
            }
        }
    }

    public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:Sinod", connectionString);
        }
    }
}
