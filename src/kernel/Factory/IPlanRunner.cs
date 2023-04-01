using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Orchestration.Extensions;

namespace Advent.Kernel.Factory;

public interface IPlanExecutor
{
    Task<SKContext> Execute(IKernel kernel, Message message, int iterations);
}

public class DefaultPlanExecutor : IPlanExecutor
{
    public async Task<SKContext> Execute(IKernel kernel, Message message, int iterations)
    {
        SKContext plan = await kernel.RunAsync(message.Variables.ToContext(), kernel.CreatePlan());
        var iteration = 0;
        var executePlan = kernel.ExecutePlan();

        var result = await kernel.RunAsync(plan.Variables, executePlan);

        while (!result.Variables.ToPlan().IsComplete && result.Variables.ToPlan().IsSuccessful &&
               iteration < iterations - 1)
        {
            result = await kernel.RunAsync(result.Variables, executePlan);
            iteration++;
        }

        return result;
    }
}

internal static partial class Extensions
{
    internal static ISKFunction CreatePlan(this IKernel kernel) =>
        kernel.Skills.GetFunction("plannerskill", "createplan");

    internal static ISKFunction ExecutePlan(this IKernel kernel) =>
        kernel.Skills.GetFunction("plannerskill", "executeplan");

    internal static ContextVariables ToContext(this IEnumerable<KeyValuePair<string, string>> variables)
    {
        var context = new ContextVariables();
        foreach (var variable in variables) context[variable.Key] = variable.Value;
        return context;
    }
}