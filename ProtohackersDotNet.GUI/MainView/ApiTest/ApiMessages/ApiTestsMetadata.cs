using ProtoHackersDotNet.GUI.MainView.ApiTest.ApiMessages;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest;

[JsonSerializable(typeof(TestApiRequest)), JsonSerializable(typeof(TestRequestApiResponse))]
[JsonSerializable(typeof(ApiCheckResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, UseStringEnumConverter = true)]
internal partial class ApiTestsMetadata : JsonSerializerContext;
