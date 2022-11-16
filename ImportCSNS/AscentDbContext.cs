using Ascent.Services;
using Microsoft.EntityFrameworkCore;

namespace ImportCSNS;

internal class AscentDbContext : AppDbContext
{
    private readonly string _connectionString;

    public AscentDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_connectionString);
}
