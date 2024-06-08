namespace ProtoHackersDotNet.GUI.MainView.Grader.Messages;

public class GraderResponse : IApiResponse
{
    [JsonPropertyName("checkstatus")]
    public required GraderStatus CheckStatus { get; init; }

    [JsonConverter(typeof(GradingDateTimeConverter))]
    public required DateTimeOffset CreatedAt { get; init; }

    [JsonConverter(typeof(GradingDateTimeConverter))]
    public DateTimeOffset? FinishedAt { get; init; }
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

public interface IApiResponse
{
    ResponseStatus Status { get; }
}