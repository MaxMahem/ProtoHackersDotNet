namespace ProtoHackersDotNet.GUI.MainView.ApiTest.ApiMessages;

public class ApiCheckResponse : IApiResponse
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

public interface IApiResponse
{
    ResponseStatus Status { get; }
}