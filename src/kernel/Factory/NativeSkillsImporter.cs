using Microsoft.SemanticKernel;

namespace Advent.Kernel.Factory;

public class NativeSkillsImporter : ISkillsImporter
{
    private readonly IList<Type> _skills;
    private readonly IServiceProvider _provider;

    public NativeSkillsImporter(SkillOptions skillOptions, IServiceProvider provider)
    {
        _skills = skillOptions.NativeSkillTypes;
        _provider = provider;
    }

    public void ImportSkills(IKernel kernel)
    {
        foreach (var skill in _skills)
        {
            var instance = _provider.GetService(skill);
            if (instance != null) kernel.ImportSkill(instance, skill.Name);
        }
    }
}

