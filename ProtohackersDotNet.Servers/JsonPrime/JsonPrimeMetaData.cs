namespace ProtoHackersDotNet.Servers.JsonPrime;

[JsonSerializable(typeof(PrimeResponse)), JsonSerializable(typeof(PrimeQuery))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class JsonPrimeMetaData : JsonSerializerContext;
