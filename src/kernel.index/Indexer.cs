using Advent.Kernel.Factory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions.Partitioning;

namespace kernel.memory;

public class Indexer
{
    private readonly Options _options;
    private readonly ILogger _logger;
    private readonly IKernel _kernel;
    private const int MaxFileSize = 2048;
    private const int MaxTokens = 1024;

    public Indexer(SemanticKernelFactory factory, Options options, ILoggerFactory logger)
    {
        _options = options;
        _logger = logger.CreateLogger("Semantic memory indexer");
        _kernel = factory.Create(new ApiKey
        {
            Chat = options.ApiKey, Embedding = options.ApiKey, Text = options.ApiKey
        });
    }

    public async Task StartAsync()
    {
        _logger.LogInformation($"Start indexing {_options.Directory}...");
        await IndexDirectoryAsync(_options.Directory);
    }

    private async Task IndexDirectoryAsync(string directory)
    {
        var filePaths =
            await Task.FromResult(Directory.GetFiles(directory, "*", SearchOption.AllDirectories));
        foreach (var file in filePaths)
            await IndexFileAsync(file, Path.GetRelativePath(directory, file));
    }

    private async Task IndexFileAsync(string path, string name)
    {
        var extension = new FileInfo(path).Extension.ToLower();
        if (!_options.Extensions.Contains(extension)) return;

        _logger.LogInformation($"Indexing {name}");

        var content = await File.ReadAllTextAsync(path);
        if (content.Length > MaxFileSize)
        {
            var paragraphs = SplitParagraphs(extension, content);
            for (var i = 0; i < paragraphs.Count; i++)
            {
                _logger.LogInformation($"Adding to collection {_options.Collection},  {name}_{i}");
                await _kernel.Memory.SaveInformationAsync($"{_options.Collection}",
                    text: $"{paragraphs[i]} File:{name}",
                    id: $"{name}_{i}");
            }
        }
        else
        {
            _logger.LogInformation($"Adding to collection {_options.Collection},  {name}");
            await _kernel.Memory.SaveInformationAsync($"{_options.Collection}", text: $"{content} File:{name}",
                id: name);
        }
    }

    private static List<string> SplitParagraphs(string extension, string content) =>
        extension switch
        {
            ".md" => SemanticTextPartitioner.SplitMarkdownParagraphs(
                SemanticTextPartitioner.SplitMarkDownLines(content, MaxTokens), MaxTokens),
            _ => SemanticTextPartitioner.SplitPlainTextParagraphs(
                SemanticTextPartitioner.SplitPlainTextLines(content, MaxTokens), MaxTokens)
        };
}