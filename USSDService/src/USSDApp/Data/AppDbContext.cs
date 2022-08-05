using Microsoft.EntityFrameworkCore;
using USSDTest.Models;
using USSDTest.Models.Common;

namespace USSDApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<PollingCenter> PollingCenters => Set<PollingCenter>();
    public DbSet<PollingStation> PollingStations => Set<PollingStation>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<BaseEntity> BaseEntities => Set<BaseEntity>();
}