using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Orchestration;
using Xunit.Abstractions;

namespace Advent.Kernel.Factory;

public class KernelTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly SemanticKernelFactory _factory;

    public KernelTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var folder = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../../../skills");
        _factory = new HostBuilder()
            .ConfigureServices(services => { services.AddSingleton<ILogger>(NullLogger.Instance); })
            .ConfigureAdventKernelDefaults(folder).Build().Services
            .GetService<SemanticKernelFactory>()!;
    }

    [Fact]
    public void should_load_all_core_skills()
    {
        var skills = _factory.Create(new()
                { Completion = new() { Label = "label", Model = "model", Key = "api-key" } }).Skills.GetFunctionsView()
            .NativeFunctions.Keys;
        Assert.Contains(nameof(FileIOSkill), skills);
        Assert.Contains(nameof(HttpSkill), skills);
        Assert.Contains(nameof(TextSkill), skills);
        Assert.Contains(nameof(TextMemorySkill), skills);
        Assert.Contains(nameof(ConversationSummarySkill), skills);
        Assert.Contains(nameof(TimeSkill), skills);
        Assert.Contains(nameof(PlannerSkill), skills);
    }


    [Fact]
    public void should_load_semantic_skills_from_skills_folder()
    {
        var skills = _factory.Create(new()
            { Completion = new() { Label = "label", Model = "model", Key = "api-key" } }).Skills;
        Assert.True(skills.HasFunction("DevSkill", "WriteCode"));
        Assert.False(skills.HasFunction("DevSkill", "NotAFunction"));
    }

    [Fact]
    public void should_load_native_skills_from_skills_folder()
    {
        var skills = _factory.Create(new()
            { Completion = new() { Label = "label", Model = "model", Key = "api-key" } }).Skills;
        Assert.True(skills.HasFunction("WebBrowserSkill", "OpenBrowserAsync"));
    }

    [Fact]
    public void should()
    {
        var variables = new ContextVariables();
        variables["INPUT"] = "input";

        string json = JsonSerializer.Serialize(variables, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
// Output the JSON string
        _testOutputHelper.WriteLine(json);
    }
}