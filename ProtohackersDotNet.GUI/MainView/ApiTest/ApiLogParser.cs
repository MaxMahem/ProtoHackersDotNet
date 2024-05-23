using ProtoHackersDotNet.GUI.MainView.ApiTest.ApiRequests;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest;

/// <summary>Encapsulates state for parsing log files from the protohacker check api.</summary>
/// <remarks><see cref="ApiCheckResponse"/> Log files include past log entires, so this handles
/// tracking what lines still need to be parsed.</remarks>
public class ApiLogParser(IServer server, Uri checkApi)
{
    const char LOG_LINE_SEPERATOR = '\n';

    int processedLogLength = 0;

    /// <summary>Processes all new log data from <paramref name="checkResponse"/></summary>
    /// <param name="checkResponse">The response to process.</param>
    /// <returns>An enumeration of all *new* TestLogMessages found in <paramref name="checkResponse"/>.</returns>
    public IEnumerable<TestLogMessage> ProcessLogs(ApiCheckResponse checkResponse)
    {
        var unseenLog = checkResponse.Log[this.processedLogLength..];
        foreach (var line in unseenLog.Split(LOG_LINE_SEPERATOR, StringSplitOptions.RemoveEmptyEntries))
            yield return new TestLogMessage(server, checkApi, line);
        this.processedLogLength = checkResponse.Log.Length;
    }
}
