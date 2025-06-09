using System.Net.Http.Json;
using System.Text.Json;
using Rekindle.Search.Application.Images.Interfaces;
using Rekindle.Search.Application.Images.Models;
using Rekindle.Search.Application.Images.Requests;

namespace Rekindle.Search.Infrastructure.Ai;

public class DeepFaceClient : IDeepFaceClient
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public DeepFaceClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<FaceAddResponse> AddFacesFromImageAsync(Guid groupId, Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/add_faces");
        byte[] imageBytes;
        using (var memoryStream = new MemoryStream())
        {
            await imageStream.CopyToAsync(memoryStream, cancellationToken);
            imageBytes = memoryStream.ToArray();
        }

        var requestBody = new AddFaceRequest(groupId.ToString(), Convert.ToBase64String(imageBytes));
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonSerializerOptions),
            System.Text.Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to add faces: {response.ReasonPhrase}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<FaceAddResponse>(JsonSerializerOptions, cancellationToken);

        if (result == null)
        {
            throw new HttpRequestException("Failed to parse response from DeepFace API.");
        }

        return result;
    }

    public async Task<Stream> GetLatestFaceByUserIdAsync(Guid groupId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"api/groups/{groupId}/users/{userId}/last_image", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to retrieve latest face: {response.ReasonPhrase}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<LastUserFaceResponse>(JsonSerializerOptions, cancellationToken);

        if (result == null)
        {
            throw new HttpRequestException("Failed to parse response from DeepFace API.");
        }

        if (string.IsNullOrEmpty(result.Image.ImageBase64))
        {
            throw new InvalidOperationException("No face image found for the specified user.");
        }

        var imageBytes = Convert.FromBase64String(result.Image.ImageBase64);
        return new MemoryStream(imageBytes);
    }

    public async Task MergeUsers(Guid groupId, IEnumerable<Guid> sourceUserIds, Guid targetUserId,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/merge_users");
        var requestBody = new MergeUsersRequest(
            groupId.ToString(),
            sourceUserIds.Select(id => id.ToString()),
            targetUserId.ToString()
        );

        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonSerializerOptions),
            System.Text.Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to merge users: {response.ReasonPhrase}");
        }
    }
}