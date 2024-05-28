using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;

[JsonSerializable(typeof(TestApiRequest)), JsonSerializable(typeof(TestRequestApiResponse))]
[JsonSerializable(typeof(ApiCheckResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, UseStringEnumConverter = true)]
internal partial class ApiTestsMetadata : JsonSerializerContext;
