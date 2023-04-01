using System.Text.Json;
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

    public static async Task<SKContext> InvokeEndToEnd(this IKernel kernel, Message message, int iterations) =>
        await ExecutePlan(kernel, await kernel.RunAsync(message.Variables.ToContext(), kernel.CreatePlan()),
            iterations);

    private static async Task<SKContext> ExecutePlan(IKernel kernel, SKContext plan, int iterations)
    {
        var iteration = 0;
        var executePlan = kernel.ExecutePlan();

        var result = await kernel.RunAsync(plan.Variables, executePlan);

        while (!result.Variables.ToPlan().IsComplete &&
               result.Variables.ToPlan().IsSuccessful &&
               iteration < iterations - 1)
        {
            result = await kernel.RunAsync(result.Variables, executePlan);
            iteration++;
        }

        return result;
    }

    private static ISKFunction CreatePlan(this IKernel kernel) =>
        kernel.Skills.GetFunction("plannerskill", "createplan");

    private static ISKFunction ExecutePlan(this IKernel kernel) =>
        kernel.Skills.GetFunction("plannerskill", "executeplan");

    private static ContextVariables ToContext(this IEnumerable<KeyValuePair<string, string>> variables)
    {
        var context = new ContextVariables();
        foreach (var variable in variables) context[variable.Key] = variable.Value;
        return context;
    }
}