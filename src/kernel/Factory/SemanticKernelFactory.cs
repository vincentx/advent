using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Configuration;
using Microsoft.SemanticKernel.CoreSkills;

namespace Advent.Kernel.Factory;

public class SemanticKernelFactory
{
    private readonly NativeSkillsImporter _native;
    private readonly SemanticSkillsImporter _semantic;
    private readonly Config _config;
    private readonly ILogger _logger;

    public SemanticKernelFactory(NativeSkillsImporter native, SemanticSkillsImporter semantic, Config config,
        ILogger logger)
    {
        _native = native;
        _semantic = semantic;
        _config = config;
        _logger = logger;
    }

    public IKernel Create(ApiKey key, IList<string>? skills = null)
    {
        var selected = (skills ?? new List<string>()).Select(_ => _.ToLower()).ToList();
        return new KernelBuilder().WithOpenAI(_config, key).WithLogger(_logger).Build()
            .RegistryCoreSkills(selected)
            .Register(_native, selected)
            .Register(_semantic, selected);
    }
}

internal static partial class Extensions
{
    internal static KernelBuilder WithOpenAI(this KernelBuilder builder, Config config, ApiKey api) =>
        builder.Configure(_ =>
        {
            if (api.Text != null) _.AddOpenAITextCompletion("completion", config.Models.Text, api.Text);
            if (api.Embedding != null)
                _.AddOpenAIEmbeddingGeneration("embedding", config.Models.Embedding, api.Embedding);
            if (api.Chat != null) _.AddOpenAIChatCompletion("chat", config.Models.Chat, api.Chat);
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