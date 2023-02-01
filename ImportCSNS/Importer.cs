using Microsoft.Extensions.Configuration;

namespace ImportCSNS;

public partial class Importer
{
    private readonly IConfiguration _config;

    public Importer()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("csnssettings.json")
            .Build();
    }
}
