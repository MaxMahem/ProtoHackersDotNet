using System.Text.Json.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest.ApiRequests;

public class ApiCheckResponse
{
    [JsonPropertyName("checkstatus")]
    public required CheckStatus CheckStatus { get; init; }

    [JsonConverter(typeof(ApiDateTimeConverter))]
    public required DateTime CreatedAt { get; init; }

    [JsonConverter(typeof(ApiDateTimeConverter))]
    public DateTime? FinishedAt { get; init; }
    public required string Hostname { get; init; }
    public required string Log { get; init; }
    public required string Message { get; init; }
    public required uint Port { get; init; }
    public required int Problem { get; init; }
    public int? Ranking { get; init; }
    public required ResponseStatus Status { get; init; }
    public required int SubmissionId { get; init; }
    public required int UserId { get; init; }
}

public static class ResultCheckResponseHelper
{
    public static DisplayMessage CompletedResponseToFormatedMessage(this ApiCheckResponse response, int problemId, Uri api)
        => new()
        {
            ProblemId = problemId,
            Source = api.ToString(),
            Timestamp = response.FinishedAt.ToString() ?? ThrowHelper.ThrowArgumentNullException<string>(),
            Type = response.Status.ToString(),
            Message = "Testing finished."
        };
}