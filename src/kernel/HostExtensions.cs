using Advent.Kernel.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Advent.Kernel;

public static class HostExtensions
{
    public static IHostBuilder ConfigureAdventKernelDefaults(this IHostBuilder builder, string folder) =>
        builder.ConfigureServices(services =>
        {
            var options = folder.ToSkillOptions();
            services.AddSingleton(options);
            foreach (var skillType in options.NativeSkillTypes) services.AddSingleton(skillType);
            services.AddSingleton<ILogger>(NullLogger.Instance);
            services.AddSingleton<NativeSkillsImporter>();
            services.AddSingleton<SemanticSkillsImporter>();
            services.AddSingleton<SemanticKernelFactory>();
        });
}