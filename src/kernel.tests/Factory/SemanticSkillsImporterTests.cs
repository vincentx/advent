using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;

namespace Advent.Kernel.Factory;

public class SemanticSkillsImporterTests
{
    [Fact]
    public void should_import_semantic_skills_from_folder()
    {
        var kernel = new KernelBuilder().Configure(c => { c.AddOpenAITextCompletionService("label", "model", "key"); })
            .Build();

        var folder = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../../../skills");
        new SemanticSkillsImporter(new SkillOptions { SemanticSkillsFolders = new[] { folder } },
            new NullLoggerFactory()).ImportSkills(kernel,
            new List<string>());

        var readOnlySkillCollection = kernel.Skills;
        Assert.True(readOnlySkillCollection.HasFunction("DevSkill", "WriteCode"));
    }

    [Fact]
    public void should_not_import_semantic_skills_from_folder_if_skill_not_selected()
    {
        var kernel = new KernelBuilder().Configure(c => { c.AddOpenAITextCompletionService("label", "model", "key"); })
            .Build();

        var folder = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../../../skills");
        new SemanticSkillsImporter(new SkillOptions { SemanticSkillsFolders = new[] { folder } },
            new NullLoggerFactory()).ImportSkills(kernel,
            new List<string> { "otherskills" });

        var readOnlySkillCollection = kernel.Skills;
        Assert.False(readOnlySkillCollection.HasFunction("DevSkill", "WriteCode"));
    }

    [Fact]
    public void should_import_semantic_skills_from_folder_if_skill_selected()
    {
        var kernel = new KernelBuilder().Configure(c => { c.AddOpenAITextCompletionService("label", "model", "key"); })
            .Build();

        var folder = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../../../skills");
        new SemanticSkillsImporter(new SkillOptions { SemanticSkillsFolders = new[] { folder } },
            new NullLoggerFactory()).ImportSkills(kernel,
            new List<string> { "devskill" });

        var readOnlySkillCollection = kernel.Skills;
        Assert.True(readOnlySkillCollection.HasFunction("DevSkill", "WriteCode"));
    }
}