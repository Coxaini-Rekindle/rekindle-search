namespace Rekindle.Search.Application.Common.Interfaces;

public interface IImageDescriptor
{
    Task<string> DescribeImageAsync(Stream image, string contentType);
}