using Advent.Kernel;
using Advent.Kernel.Factory;
using kernel.memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var config = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json", true)
    .AddJsonFile("advent.json", true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = config.GetSection("OPENAI_API_KEY").Get<string>();

if (apiKey == null)
{
    Environment.ExitCode = 1;
    return -1;
}

var options = new Options
{
    Collection = args[0],
    Directory = args[1],
    ApiKey = apiKey,
    Extensions = args.Length > 2
        ? new ArraySegment<string>(args, 2, args.Length - 2)
            .Select(_ => _.ToLower()).ToArray()
        : new[] { ".md", ".txt" }
};

var app = Host.CreateDefaultBuilder(args).ConfigureAdventKernelDefaults(config)
    .ConfigureServices(services =>
    {
        services.AddSingleton<Indexer>()
            .AddSingleton(options)
            .AddSingleton(config);
    }).UseConsoleLifetime().Build();
await app.Services.GetService<Indexer>()!.StartAsync();
return 0;