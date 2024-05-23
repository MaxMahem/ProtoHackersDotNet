using System.Globalization;
using System.Text.RegularExpressions;
using ProtoHackersDotNet.GUI.MainView.ApiTest.Messages;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest;

public partial class TestLogMessage : TestEvent
{
    public TestLogMessage(IServer server, Uri api, string log) : base(server, api)
    {
        (Group timestamp, Message) = LogRegex().Matches(log) switch {
            [{ Groups: [_, var time, var test, var status, { Success: true } mess] }] => (time, $"{test} - {status}: {mess}"),
            [{ Groups: [_, var time, var test, var status, _] }]                      => (time, $"{test} - {status}"),
            _ => ThrowHelper.ThrowFormatException<ValueTuple<Group, string>>($"'{log}' failed parsing.")
        };
        Timestamp = DateTime.ParseExact(timestamp.ValueSpan, "ddd MMM dd HH:mm:ss yyyy 'UTC'", CultureInfo.InvariantCulture, 
            DateTimeStyles.AssumeUniversal).ToUniversalTime();
    }

    public override DateTime Timestamp { get; protected set; }

    public override DisplayMessageType MessageType => DisplayMessageType.TestLog;

    public override string Message { get; }

    [GeneratedRegex(@"\[(.*?)\] \[(.*?)\] (\w+):?(.+)?")]
    private static partial Regex LogRegex();
}