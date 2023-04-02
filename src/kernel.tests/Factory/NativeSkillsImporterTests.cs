using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Advent.Kernel.Factory;

public class NativeSkillsImporterTests
{
    [Fact]
    public void should_create_skills_from_service_provider()
    {
        var kernel = new KernelBuilder().Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(TestNativeSkill), typeof(TestNativeSkill));
        
        new NativeSkillsImporter(new SkillOptions { NativeSkillTypes = new List<Type> { typeof(TestNativeSkill) } },
                serviceCollection.BuildServiceProvider())
            .ImportSkills(kernel, new List<string>());

        var readOnlySkillCollection = kernel.Skills;
        Assert.True(readOnlySkillCollection.HasFunction("TestNativeSkill", "Func"));
    }

    [Fact]
    public void should_not_import_skills_if_skill_not_selected()
    {
        var kernel = new KernelBuilder().Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(TestNativeSkill), typeof(TestNativeSkill));


        new NativeSkillsImporter(new SkillOptions { NativeSkillTypes = new List<Type> { typeof(TestNativeSkill) } },
                serviceCollection.BuildServiceProvider())
            .ImportSkills(kernel, new List<string> { "otherskill" });

        var readOnlySkillCollection = kernel.Skills;
        Assert.False(readOnlySkillCollection.HasFunction("TestNativeSkill", "Func"));
    }

    [Fact]
    public void should_import_skills_if_skill_selected()
    {
        var kernel = new KernelBuilder().Build();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(typeof(TestNativeSkill), typeof(TestNativeSkill));


        new NativeSkillsImporter(new SkillOptions { NativeSkillTypes = new List<Type> { typeof(TestNativeSkill) } },
                serviceCollection.BuildServiceProvider())
            .ImportSkills(kernel, new List<string> { "testnativeskill" });

        var readOnlySkillCollection = kernel.Skills;
        Assert.True(readOnlySkillCollection.HasFunction("TestNativeSkill", "Func"));
    }
}

public class TestNativeSkill
{
    [SKFunction("Func")]
    public async Task Func()
    {
    }
}