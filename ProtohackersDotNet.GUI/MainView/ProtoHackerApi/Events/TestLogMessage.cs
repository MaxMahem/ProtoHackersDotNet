using System.Globalization;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public partial class TestLogMessage : TestEvent
{
    public TestLogMessage(IServer server, Uri api, string log) : base(server, api)
    {
        (Group timestamp, Group status, Message) = LogRegex().Matches(log) switch {
            [{ Groups: [_, var time, var test, var stat, { Success: true } mess] }] => (time, stat, $"{test} - {stat}: {mess}"),
            [{ Groups: [_, var time, var test, var stat, _] }]                      => (time, stat, $"{test} - {stat}"),
            _ => ThrowFormatException<(Group, Group, string)>($"'{log}' failed parsing")
        };
        Timestamp = DateTimeOffset.ParseExact(timestamp.ValueSpan, "ddd MMM d HH:mm:ss yyyy 'UTC'", CultureInfo.InvariantCulture, 
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowInnerWhite);
        MessageType = status.ValueSpan switch {
            "PASS" => MessageType.Success,
            "FAIL" => MessageType.Error,
            "NOTE" => MessageType.Notice,
            _ => ThrowFormatException<MessageType>($"Unknown log message status '{status.ValueSpan}'")
        };
    }

    public override string Message { get; }
    public override MessageType MessageType { get; }
    public override string Type => nameof(TestLogMessage);

    [GeneratedRegex(@"\[(.*?)\] \[(.*?)\] (\w+):?(.+)?")]
    private static partial Regex LogRegex();
}