using Advent.Kernel.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Orchestration.Extensions;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Advent.Kernel;

public static class Extensions
{
    public static IHostBuilder ConfigureAdventKernelDefaults(this IHostBuilder builder, string folder) =>
        builder.ConfigureServices(services => { services.AddSemanticKernelFactory(folder); });

    public static void AddSemanticKernelFactory(this IServiceCollection services, string folder)
    {
        services.AddSingleton<ILogger>(NullLogger.Instance);

        var options = folder.ToSkillOptions();
        services.AddSingleton(options);
        foreach (var skillType in options.NativeSkillTypes) services.AddSingleton(skillType);
        services.AddSingleton<NativeSkillsImporter>();
        services.AddSingleton<SemanticSkillsImporter>();
        services.AddSingleton<SemanticKernelFactory>();
        services.AddSingleton(typeof(IPlanExecutor), typeof(DefaultPlanExecutor));
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