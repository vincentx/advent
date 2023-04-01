using System.Reflection;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Advent.Kernel.Factory;

public class ApiKeyConfig
{
    public Config Completion { get; init; } = new();

    public Config Embedding { get; init; } = new();

    public class Config
    {
        public string Model { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;

        public bool IsValid() =>
            !string.IsNullOrEmpty(Label) && !string.IsNullOrEmpty(Model) && !string.IsNullOrEmpty(Key);
    }
}

public class SkillOptions
{
    public string SemanticSkillsFolder { get; init; } = "./skills";

    public IList<Type> NativeSkillTypes { get; init; } = new List<Type>();
}

internal static partial class Extensions
{
    public static SkillOptions ToSkillOptions(this string directory) =>
        new()
        {
            SemanticSkillsFolder = directory,
            NativeSkillTypes = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories)
                .SelectMany(file => Assembly.LoadFrom(file).GetTypes().Where(_ =>
                    _.GetMethods().Any(m => m.GetCustomAttribute<SKFunctionAttribute>() != null))).ToList()
        };
}