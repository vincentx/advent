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

    public void ImportSkills(IKernel kernel)
    {
        kernel.RegisterSemanticSkills(_folder);
    }
}

internal static partial class Extensions
{
    internal static IKernel RegisterSemanticSkills(this IKernel kernel, string skill)
    {
        foreach (var prompt in Directory.EnumerateFiles(skill, "*.txt", SearchOption.AllDirectories)
                     .Select(_ => new FileInfo(_)))
            kernel.ImportSemanticSkillFromDirectory(skill, FunctionName(skill, prompt.Directory));
        return kernel;
    }

    private static string FunctionName(string skill, DirectoryInfo? folder)
    {
        while (folder?.Parent?.FullName != skill) folder = folder?.Parent;
        return folder.Name;
    }
}