using Microsoft.EntityFrameworkCore;
using Sinod.Api.Entities;

namespace Sinod.Api.Data;

public class SinodDbContext(DbContextOptions<SinodDbContext> options) : DbContext(options)
{
    // Here we add the DbSet<T> to register our business entities.
    public DbSet<OdClient>  OdClients { get; set; }
    public DbSet<OdService>  OdServices { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<OdEvent> OdEvents { get; set; }
    public DbSet<OdServicePricingProfile> OdServicePricingProfiles { get; set; }
    public DbSet<Deposit> Deposits { get; set; }
    
    // Method that executes in DbContext when building the models.
    // We override the default behavior
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Opportunity>().Property(o => o.UpdatedAt).IsConcurrencyToken();
        modelBuilder.Entity<OdEvent>().HasIndex(e => e.OpportunityId).IsUnique();
        modelBuilder.Entity<OdEvent>().HasOne(e => e.Quotation).WithMany()
            .HasForeignKey(e => e.QuotationId);
        modelBuilder.Entity<Quotation>().HasIndex(q => new { q.OpportunityId, q.Version }).IsUnique();
        modelBuilder.Entity<Quotation>().HasMany(q => q.Items).WithOne()
            .HasForeignKey(i => i.QuotationId);
        modelBuilder.Entity<Quotation>().HasIndex(q => q.OpportunityId).IsUnique()
            .HasFilter("\"State\" = 2").HasDatabaseName("IX_Quotations_OneAcceptedPerOpportunity");

        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .Where(e => e.ClrType != typeof(Company))
                     .SelectMany(e => e.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(12);
            property.SetScale(2);
        }

        modelBuilder.Entity<Opportunity>(entity =>
        {
            // Mark the Location properties as Complex Types
            // This way, EF adds them to "Opportunities" Entity table automatically
            entity.ComplexProperty(o => o.Location, location =>
            {
                location.HasDiscriminator();
                location.ComplexProperty(l => l.Coordinates);
            });
            
            #region Define Opportunity Relations
            entity.HasOne(o => o.Client)
                .WithMany()
                .HasForeignKey(o => o.ClientId);

            entity.HasMany(o => o.Services)
                .WithMany()
                .UsingEntity("OpportunityServices");
            #endregion
        });

        // Historical records must never disappear through a cascading delete.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
