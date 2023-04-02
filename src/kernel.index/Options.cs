namespace kernel.memory;

public class Options
{
    public string ApiKey { get; init; } = "";
    public string[] Extensions { get; init; } = { ".md", ".txt" };
    public string Collection { get; init; } = "knowledge_base";
    public string Directory { get; init; } = "./knowledge_base";
}