using Advent.Kernel.Factory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;

namespace Advent.Kernel.WebApi;

public static class Extensions
{
    public static ApiKeyConfig ToApiKeyConfig(this HttpRequest request)
    {
        var apiConfig = new ApiKeyConfig();
        if (request.Headers.TryGetValue(Headers.CompletionModel, out var completionModelValue))
        {
            apiConfig.Completion.Model = completionModelValue.First()!;
            apiConfig.Completion.Label = apiConfig.Completion.Model;
        }

        if (request.Headers.TryGetValue(Headers.CompletionKey, out var completionKey))
        {
            apiConfig.Completion.Key = completionKey.First()!;
        }

        if (request.Headers.TryGetValue(Headers.EmbeddingModel, out var embeddingModelValue))
        {
            apiConfig.Embedding.Model = embeddingModelValue.First()!;
            apiConfig.Embedding.Label = apiConfig.Embedding.Model;
        }

        if (request.Headers.TryGetValue(Headers.EmbeddingKey, out var embeddingKey))
        {
            apiConfig.Embedding.Key = embeddingKey.First()!;
        }

        return apiConfig;
    }

    public static bool TryGetKernel(this HttpRequest request, SemanticKernelFactory factory,
        out IKernel? kernel, IList<string>? selected = null)
    {
        var apiConfig = request.ToApiKeyConfig();
        kernel = apiConfig.Completion.IsValid() ? factory.Create(apiConfig, selected) : null;
        return kernel != null;
    }

    public static IResult ToResult(this SKContext result, IList<string>? skills) => (result.ErrorOccurred)
        ? Results.BadRequest(result.LastErrorDescription)
        : Results.Ok(new Message { Variables = result.Variables, Skills = skills ?? new List<string>()});
}