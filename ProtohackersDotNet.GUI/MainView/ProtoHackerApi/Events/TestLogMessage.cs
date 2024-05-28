using System.Globalization;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public partial class TestLogMessage : TestEvent
{
    public TestLogMessage(IServer server, Uri api, string log) : base(server, api)
    {
        (Group timestamp, Message) = LogRegex().Matches(log) switch {
            [{ Groups: [_, var time, var test, var status, { Success: true } mess] }] => (time, $"{test} - {status}: {mess}"),
            [{ Groups: [_, var time, var test, var status, _] }]                      => (time, $"{test} - {status}"),
            _ => ThrowHelper.ThrowFormatException<(Group, string)>($"'{log}' failed parsing")
        };
        Timestamp = DateTimeOffset.ParseExact(timestamp.ValueSpan, "ddd MMM dd HH:mm:ss yyyy 'UTC'", CultureInfo.InvariantCulture, 
            DateTimeStyles.AssumeUniversal).ToUniversalTime();
    }

    public override string Message { get; }
    public override MessageCategory Category => MessageCategory.Notice;
    public override string Type => nameof(TestLogMessage);

    [GeneratedRegex(@"\[(.*?)\] \[(.*?)\] (\w+):?(.+)?")]
    private static partial Regex LogRegex();
}