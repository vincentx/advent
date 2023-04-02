using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;

namespace Advent.Kernel.Factory;

public class SemanticSkillsImporter : ISkillsImporter
{
    private readonly string[] _folders;
    private readonly ILogger<SemanticSkillsImporter> _logger;

    public SemanticSkillsImporter(SkillOptions skillOptions, ILoggerFactory logger)
    {
        _folders = skillOptions.SemanticSkillsFolders;
        _logger = logger.CreateLogger<SemanticSkillsImporter>();
    }

    public void ImportSkills(IKernel kernel, IList<string> skills)
    {
        foreach (var folder in _folders)
            kernel.RegisterSemanticSkills(folder, skills, _logger);
    }
}

internal static partial class Extensions
{
    internal static void RegisterSemanticSkills(this IKernel kernel, string skill, IList<string> skills, ILogger logger)
    {
        foreach (var prompt in Directory.EnumerateFiles(skill, "*.txt", SearchOption.AllDirectories)
                     .Select(_ => new FileInfo(_)))
        {
            logger.LogDebug($"{prompt} === ");
            logger.LogDebug($"{skill} === ");
            logger.LogDebug($"{prompt.Directory?.Parent} === ");
            var skillName = FunctionName(new DirectoryInfo(skill), prompt.Directory);
            logger.LogDebug($"{skillName} === ");
            if (skills.Count != 0 && !skills.Contains(skillName.ToLower())) continue;
            logger.LogDebug($"Importing semantic skill ${skill}/${skillName}");
            kernel.ImportSemanticSkillFromDirectory(skill, skillName);
        }
    }

    private static string FunctionName(DirectoryInfo skill, DirectoryInfo? folder)
    {
        while (!skill.FullName.Equals(folder?.Parent?.FullName)) folder = folder?.Parent;
        return folder.Name;
    }
}