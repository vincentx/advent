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

    public IKernel Create(ApiKeyConfig config)
    {
        return new KernelBuilder().WithOpenAI(config).WithLogger(_logger).Build()
            .RegistryCoreSkills()
            .Register(_native)
            .Register(_semantic);
    }
}

internal static partial class Extensions
{
    internal static KernelBuilder WithOpenAI(this KernelBuilder builder, ApiKeyConfig config) =>
        builder.Configure(_ =>
        {
            _.AddOpenAITextCompletion(config.Completion.Label, config.Completion.Model, config.Completion.Key);
        });

    internal static IKernel Register(this IKernel kernel, ISkillsImporter importer)
    {
        importer.ImportSkills(kernel);
        return kernel;
    }

    public static IKernel RegistryCoreSkills(this IKernel kernel)
    {
        kernel.ImportSkill(new FileIOSkill(), nameof(FileIOSkill));
        kernel.ImportSkill(new HttpSkill(), nameof(HttpSkill));
        kernel.ImportSkill(new TextSkill(), nameof(TextSkill));
        kernel.ImportSkill(new TextMemorySkill(), nameof(TextMemorySkill));
        kernel.ImportSkill(new ConversationSummarySkill(kernel), nameof(ConversationSummarySkill));
        kernel.ImportSkill(new TimeSkill(), nameof(TimeSkill));
        kernel.ImportSkill(new PlannerSkill(kernel), nameof(PlannerSkill));
        return kernel;
    }
}