using ProtoHackersDotNet.Servers.JsonPrime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProtoHackersDotNet.Servers.JsonPrime;

// [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(PrimeResponse)), JsonSerializable(typeof(PrimeQuery))]
internal partial class JsonPrimeSourceGenerationContext : JsonSerializerContext
{
    public static readonly JsonSerializerOptions JsonPrimeOptions = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = Default
    };

    static CachedUtf8Response SerializeResponse<T>(T value ) 
        => new(JsonSerializer.Serialize(value, JsonPrimeOptions) + (char) JsonPrimeClient.LINE_DELIMITER);

    public static readonly CachedUtf8Response Malformed = new("malformed" + (char) JsonPrimeClient.LINE_DELIMITER);
    public static readonly CachedUtf8Response IsPrime  = SerializeResponse<PrimeResponse>(new(true));
    public static readonly CachedUtf8Response NotPrime = SerializeResponse<PrimeResponse>(new(false));
}
