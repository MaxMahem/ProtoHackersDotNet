using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProtoHackersDotNet.Servers.JsonPrime;

[JsonSerializable(typeof(PrimeResponse)), JsonSerializable(typeof(PrimeQuery))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class JsonPrimeMetaData : JsonSerializerContext;
