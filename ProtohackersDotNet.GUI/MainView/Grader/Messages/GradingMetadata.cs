using ProtoHackersDotNet.GUI.MainView.Grader.Messages;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

/// <summary>Metadata for Json serialization of Grading Client responses.</summary>
[JsonSerializable(typeof(GradingRequest)), JsonSerializable(typeof(GradingRequestResponse))]
[JsonSerializable(typeof(GraderResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, UseStringEnumConverter = true)]
internal partial class GradingMetadata : JsonSerializerContext;