using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Configuration;
using Microsoft.SemanticKernel.CoreSkills;

namespace Advent.Kernel.Factory;

public class SemanticKernelFactory
{
    private readonly NativeSkillsImporter _native;
    private readonly SemanticSkillsImporter _semantic;
    private readonly ILogger _logger;

    public SemanticKernelFactory(NativeSkillsImporter native, SemanticSkillsImporter semantic,
        ILogger logger)
    {
        _native = native;
        _semantic = semantic;
        _logger = logger;
    }

    public IKernel Create(ApiKeyConfig config, IList<string>? skills = null)
    {
        var selected = (skills ?? new List<string>()).Select(_ => _.ToLower()).ToList();
        return new KernelBuilder().WithOpenAI(config).WithLogger(_logger).Build()
            .RegistryCoreSkills(selected)
            .Register(_native, selected)
            .Register(_semantic, selected);
    }
}

internal static partial class Extensions
{
    internal static KernelBuilder WithOpenAI(this KernelBuilder builder, ApiKeyConfig config) =>
        builder.Configure(_ =>
        {
            _.AddOpenAITextCompletion(config.Completion.Label, config.Completion.Model, config.Completion.Key);
        });

    internal static IKernel Register(this IKernel kernel, ISkillsImporter importer, IList<string> skills)
    {
        importer.ImportSkills(kernel, skills);
        return kernel;
    }

    public static IKernel RegistryCoreSkills(this IKernel kernel, IList<string> skills)
    {
        if (ShouldLoad(skills, nameof(FileIOSkill))) kernel.ImportSkill(new FileIOSkill(), nameof(FileIOSkill));
        if (ShouldLoad(skills, nameof(HttpSkill))) kernel.ImportSkill(new HttpSkill(), nameof(HttpSkill));
        if (ShouldLoad(skills, nameof(TextSkill))) kernel.ImportSkill(new TextSkill(), nameof(TextSkill));
        if (ShouldLoad(skills, nameof(TextMemorySkill)))
            kernel.ImportSkill(new TextMemorySkill(), nameof(TextMemorySkill));
        if (ShouldLoad(skills, nameof(ConversationSummarySkill)))
            kernel.ImportSkill(new ConversationSummarySkill(kernel), nameof(ConversationSummarySkill));
        if (ShouldLoad(skills, nameof(TimeSkill))) kernel.ImportSkill(new TimeSkill(), nameof(TimeSkill));

        kernel.ImportSkill(new PlannerSkill(kernel), nameof(PlannerSkill));
        return kernel;
    }

    private static bool ShouldLoad(IList<string> skills, string skill) =>
        skills.Count == 0 || skills.Contains(skill.ToLower());
}