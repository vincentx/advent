using Advent.Kernel.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Advent.Kernel;

public class KernelExtensionTests
{
    [Fact]
    public async Task should_invoke_function_from_kernel()
    {
        var result = await CreateKernel(typeof(TestNativeSkill)).InvokePipedFunctions(new()
        {
            Variables = new Dictionary<string, string> { ["INPUT"] = "INPUT" },
            Pipeline = new List<Message.FunctionRef>() { new() { Skill = "TestNativeSkill", Name = "Func" } }
        });
        Assert.Equal("INPUTRESULT", result.Result);
    }

    [Fact]
    public async Task should_invoke_piped_function_from_kernel()
    {
        var result = await CreateKernel(typeof(TestNativeSkill)).InvokePipedFunctions(new()
        {
            Pipeline = new List<Message.FunctionRef>
            {
                new() { Skill = "TestNativeSkill", Name = "Func" }, new() { Skill = "TestNativeSkill", Name = "Func" }
            }
        });
        Assert.Equal("RESULTRESULT", result.Result);
    }

    [Fact]
    public async Task should_execute_end_to_end_if_piped_function_not_provided()
    {
        var planner = new PlannerSkill(Plan("key", "id", "goal", false, false, ""),
            Plan("key", "id", "goal", true, true, ""));
        var result = await CreateKernel(typeof(TestNativeSkill), planner)
            .InvokeEndToEnd(new(), 3);

        Assert.True(planner.Planned);
        Assert.Equal(1, planner.Executed);
    }

    [Fact]
    public async Task should_execute_end_to_end_at_most_n_times_if_not_successful()
    {
        var planner = new PlannerSkill(Plan("key", "id", "goal", false, false, ""),
            Plan("key", "id", "goal", false, true, ""),
            Plan("key", "id", "goal", false, true, ""),
            Plan("key", "id", "goal", false, true, ""),
            Plan("key", "id", "goal", false, true, ""),
            Plan("key", "id", "goal", true, true, "")
        );
        var result = await CreateKernel(typeof(TestNativeSkill), planner)
            .InvokeEndToEnd(new(), 3);

        Assert.True(planner.Planned);
        Assert.Equal(3, planner.Executed);
    }

    [Fact]
    public async Task should_stop_executing_end_to_end_if_any_iteration_failed()
    {
        var planner = new PlannerSkill(Plan("key", "id", "goal", false, false, ""),
            Plan("key", "id", "goal", false, true, ""),
            Plan("key", "id", "goal", false, false, ""),
            Plan("key", "id", "goal", false, true, ""),
            Plan("key", "id", "goal", false, true, ""),
            Plan("key", "id", "goal", true, true, "")
        );
        var result = await CreateKernel(typeof(TestNativeSkill), planner)
            .InvokeEndToEnd(new(), 3);

        Assert.True(planner.Planned);
        Assert.Equal(2, planner.Executed);
    }

    private static IKernel CreateKernel(Type skill, PlannerSkill? planner = null)
    {
        var kernel = new KernelBuilder().Build();
        var skills = new List<Type> { skill };
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(skill, skill);

        if (planner != null)
        {
            serviceCollection.AddSingleton(typeof(PlannerSkill), planner);
            skills.Add(typeof(PlannerSkill));
        }

        new NativeSkillsImporter(new SkillOptions { NativeSkillTypes = skills },
                serviceCollection.BuildServiceProvider())
            .ImportSkills(kernel, new List<string>());
        return kernel;
    }

    private ContextVariables Plan(string key, string id, string goal, bool complete, bool successful,
        string result)
    {
        var variables = new ContextVariables();
        variables["PLAN__PLAN"] = key;
        variables["PLAN__ID"] = id;
        variables["PLAN__GOAL"] = goal;
        variables["PLAN__ISCOMPLETE"] = complete.ToString();
        variables["PLAN__ISSUCCESSFUL"] = successful.ToString();
        variables["PLAN__RESULT"] = result;

        return variables;
    }
}

public class TestNativeSkill
{
    [SKFunction("Func")]
    [SKFunctionInput(Description = "INPUT")]
    public async Task<string> Func(string input)
    {
        return $"{input}RESULT";
    }
}

public class PlannerSkill
{
    private readonly ContextVariables _plan;
    private readonly IList<ContextVariables> _results;

    internal bool Planned { get; private set; } = false;
    internal int Executed { get; private set; } = 0;

    public PlannerSkill(ContextVariables plan, params ContextVariables[] results)
    {
        _plan = plan;
        _results = new List<ContextVariables>(results);
    }

    [SKFunction("Func")]
    [SKFunctionInput(Description = "INPUT")]
    public async Task CreatePlan(string input, SKContext context)
    {
        Planned = true;
        foreach (var variable in _plan.ToList())
            context[variable.Key] = variable.Value;
    }

    [SKFunction("Func")]
    [SKFunctionInput(Description = "INPUT")]
    public async Task ExecutePlan(string input, SKContext context)
    {
        foreach (var variable in _results[Executed].ToList())
            context[variable.Key] = variable.Value;
        Executed++;
    }
}