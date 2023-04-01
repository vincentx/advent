using Microsoft.SemanticKernel;

namespace Advent.Kernel.Factory;

public interface ISkillsImporter
{
    void ImportSkills(IKernel kernel, IList<string> skills);
}