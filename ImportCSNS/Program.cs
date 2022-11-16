using Ascent.Services;
using ImportCSNS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("csnssettings.json")
    .Build();

// See https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ on how to create DbContext

using var csnsDb = new CsnsDbContext(config.GetConnectionString("CsnsConnection"));

var appDbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(config.GetConnectionString("DefaultConnection"))
    .Options;
using var ascentDb = new AppDbContext(appDbContextOptions);

Console.WriteLine($"There are {csnsDb.Projects.Count()} in CSNS.");
Console.WriteLine($"There are {ascentDb.Projects.Count()} in Ascent.");
