using Microsoft.EntityFrameworkCore;
using Sinod.Api.Entities;

namespace Sinod.Api.Data;

public class SinodDbContext(DbContextOptions<SinodDbContext> options) : DbContext(options)
{
    // Here we add the DbSet<T> to register our business entities.
    public DbSet<OdClient>  OdClients { get; set; }
    public DbSet<OdService>  OdServices { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<OdServicePricingProfile> OdServicePricingProfiles { get; set; }
}
