using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Configuration;

namespace Advent.Kernel.Factory;

public class SemanticSkillsImporterTests
{
    [Fact]
    public void should_import_semantic_skills_from_folder()
    {
        var kernel = new KernelBuilder().Configure(c => { c.AddOpenAITextCompletion("label", "model", "key"); })
            .Build();

        var folder = Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../../../skills");
        new SemanticSkillsImporter(new SkillOptions { SemanticSkillsFolder = folder }).ImportSkills(kernel);

        var readOnlySkillCollection = kernel.Skills;
        Assert.True(readOnlySkillCollection.HasFunction("DevSkill", "WriteCode"));
    }
}