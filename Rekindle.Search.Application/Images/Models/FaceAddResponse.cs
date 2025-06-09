using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rekindle.Search.Application.Images.Models;

public class FaceAddResponse
{
    public string Status { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public IEnumerable<FaceResponse> Faces { get; set; } = null!;
}

public class FaceResponse
{
    public string PersonId { get; set; } = null!;
    public double Confidence { get; set; }
    public string FaceImageBase64 { get; set; } = null!;

    [JsonConverter(typeof(FaceRecognitionTypeJsonConverter))]
    public FaceRecognitionType RecognitionType { get; set; }
}

public enum FaceRecognitionType
{
    Recognized,
    TempUser,
    Unknown
}

public class FaceRecognitionTypeJsonConverter : JsonConverter<FaceRecognitionType>
{
    public override FaceRecognitionType Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "recognized" => FaceRecognitionType.Recognized,
            "temp_user" => FaceRecognitionType.TempUser,
            "unknown" => FaceRecognitionType.Unknown,
            _ => throw new JsonException($"Unknown value for FaceRecognitionType: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, FaceRecognitionType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}