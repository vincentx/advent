using System.Collections;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;

namespace Advent.Kernel.Factory;

public class SemanticSkillsImporter : ISkillsImporter
{
    private readonly string _folder;

    public SemanticSkillsImporter(SkillOptions skillOptions)
    {
        _folder = skillOptions.SemanticSkillsFolder;
    }

    public void ImportSkills(IKernel kernel, IList<string> skills)
    {
        kernel.RegisterSemanticSkills(_folder, skills);
    }
}

internal static partial class Extensions
{
    internal static void RegisterSemanticSkills(this IKernel kernel, string skill, IList<string> skills)
    {
        foreach (var prompt in Directory.EnumerateFiles(skill, "*.txt", SearchOption.AllDirectories)
                     .Select(_ => new FileInfo(_)))
        {
            var skillName = FunctionName(skill, prompt.Directory);
            if (skills.Count == 0 || skills.Contains(skillName.ToLower()))
                kernel.ImportSemanticSkillFromDirectory(skill, skillName);
        }
    }

    private static string FunctionName(string skill, DirectoryInfo? folder)
    {
        while (folder?.Parent?.FullName != skill) folder = folder?.Parent;
        return folder.Name;
    }
}