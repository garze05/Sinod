using Microsoft.EntityFrameworkCore;

namespace Sinod.Api.Data;

public class SinodDbContext(DbContextOptions<SinodDbContext> options) : DbContext(options)
{
    // Here we add the DbSet<T> to register our business entities.
}
