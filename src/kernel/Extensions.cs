using Advent.Kernel.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Advent.Kernel;

public static class Extensions
{
    public static IHostBuilder ConfigureAdventKernelDefaults(this IHostBuilder builder, IConfiguration configuration) =>
        builder.ConfigureServices(services =>
        {
            services.AddSemanticKernelFactory(configuration);
            services.AddConsoleLogger(configuration);
        });

    public static void AddSemanticKernelFactory(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new Config();
        configuration.Bind(config);

        var options = config.Skills.ToSkillOptions();
        foreach (var skillType in options.NativeSkillTypes) services.AddSingleton(skillType);

        services.AddSingleton(options);
        services.AddSingleton(config);
        services.AddSingleton<NativeSkillsImporter>();
        services.AddSingleton<SemanticSkillsImporter>();
        services.AddSingleton<SemanticKernelFactory>();
        services.AddSingleton(typeof(IPlanExecutor), typeof(DefaultPlanExecutor));
        services.AddSingleton<IMemoryStore<float>>(new VolatileMemoryStore());
    }

    public static void AddConsoleLogger(this IServiceCollection services, IConfiguration configuration)
    {
        ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });
        services.AddSingleton<ILogger>(factory.CreateLogger<object>());
    }

    public static IList<FunctionView> ToSkills(this IKernel kernel)
    {
        var view = kernel.Skills.GetFunctionsView();
        return view.NativeFunctions.Values.SelectMany(Enumerable.ToList)
            .Union(view.SemanticFunctions.Values.SelectMany(Enumerable.ToList)).ToList();
    }

    public static async Task<SKContext> InvokePipedFunctions(this IKernel kernel, Message message) =>
        await kernel.RunAsync(message.Variables.ToContext(),
            (message.Pipeline?.Select(_ => kernel.Skills.GetFunction(_.Skill, _.Name)) ?? Array.Empty<ISKFunction>())
            .ToArray());
}