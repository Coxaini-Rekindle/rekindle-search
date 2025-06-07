using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Rekindle.Search.Application.Common.Interfaces;

namespace Rekindle.Search.Infrastructure.Ai;

public class ImageDescriptor : IImageDescriptor
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<ImageDescriptor> _logger;

    private const string Prompt =
        "Describe this family photo in as much literal and visual detail as possible for use in a vector search system." +
        " Include number of people, their apparent ages, genders, facial expressions, clothing, positions, actions," +
        " interactions, visible emotions, setting or background, objects, and any visible text. Do not interpret—only" +
        " describe what is visually present. Write all in one paragraph without line breaks or bullet points.";

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