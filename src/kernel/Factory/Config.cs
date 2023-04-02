using System.Reflection;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Advent.Kernel.Factory;

public class ApiKey
{
    public string? Text { get; set; }
    public string? Chat { get; set; }
    public string? Embedding { get; set; }
}

public class Config
{
    public string[] Skills { get; set; } = { "./skills" };

    public LanguageModel Models { get; set; } = new();

    public class LanguageModel
    {
        public string Text { get; set; } = "text-davinci-003";
        public string Chat { get; set; } = "gpt-3.5-turbo";
        public string Embedding { get; set; } = "text-embedding-ada-002";
    }
}

public class SkillOptions
{
    public string[] SemanticSkillsFolders { get; init; } = { "./skills" };

    public IList<Type> NativeSkillTypes { get; init; } = new List<Type>();
}

internal static partial class Extensions
{
    public static SkillOptions ToSkillOptions(this string[] directories) =>
        new()
        {
            SemanticSkillsFolders = directories,
            NativeSkillTypes = directories.SelectMany(_ => Directory
                .EnumerateFiles(_, "*.dll", SearchOption.AllDirectories)
                .SelectMany(file => Assembly.LoadFrom(file).GetTypes().Where(_ =>
                    _.GetMethods().Any(m => m.GetCustomAttribute<SKFunctionAttribute>() != null)))).ToList()
        };
}