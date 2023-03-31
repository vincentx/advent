using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel.CoreSkills;

namespace Advent.Kernel.Factory;

public class KernelTests
{
    private readonly SemanticKernelFactory _factory;

    public KernelTests()
    {
        var folder = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../../../skills");
        _factory = new HostBuilder().ConfigureAdventKernelDefaults(folder).Build().Services
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
}