using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Rekindle.Search.Application.Common.Interfaces;

namespace Rekindle.Search.Infrastructure.Ai;

public class ImageDescriptor : IImageDescriptor
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<ImageDescriptor> _logger;

    private const string Prompt =
        "Describe this image with specific visual details for search. Include: people (count, ages, expressions, " +
        "clothing), setting (indoor/outdoor, room type, lighting), background (walls, windows, scenery), objects " +
        "(furniture, items, decorations), activities, colors, and any visible text. Be literal and specific. " +
        "Do not describe anything that is not clearly visible in the image.";

    public ImageDescriptor(IChatClient chatClient, ILogger<ImageDescriptor> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<string> DescribeImageAsync(Stream image, string contentType)
    {
        var message = new ChatMessage(ChatRole.User, Prompt);
        var stream = new MemoryStream();
        await image.CopyToAsync(stream);
        stream.Position = 0;
        var bytes = new byte[stream.Length];
        await stream.ReadExactlyAsync(bytes);
        message.Contents.Add(new DataContent(bytes, contentType));

        var response = await _chatClient.GetResponseAsync(message);

        _logger.LogInformation("Image description response: {Response}", response.Text);

        return response.Text;
    }
}