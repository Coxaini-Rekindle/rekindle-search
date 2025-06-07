namespace Rekindle.Search.Application.Common.Interfaces;

public interface IVectorEmbeddingGenerator
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}