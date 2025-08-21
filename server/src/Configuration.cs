using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Resweet;

public static class Configuration
{
    private static IConfigurationRoot configuration;

    public static string ConnectionString => configuration.GetConnectionString("DataConnection");

    public static void Initialize()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();
    }
}
