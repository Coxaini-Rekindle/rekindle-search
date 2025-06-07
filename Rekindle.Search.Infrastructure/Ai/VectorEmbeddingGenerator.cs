using Microsoft.Extensions.AI;
using Rekindle.Search.Application.Common.Interfaces;

namespace Rekindle.Search.Infrastructure.Ai;

public class VectorEmbeddingGenerator : IVectorEmbeddingGenerator
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public VectorEmbeddingGenerator(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text,
        CancellationToken cancellationToken = default)
    {
        var result = await _embeddingGenerator.GenerateVectorAsync(text, cancellationToken: cancellationToken);

        return result;
    }
}