using Spectre.Console;
using ProtoHackersDotNet.Servers.Interface.Client;
using ProtoHackersDotNet.Servers.Interface.Server;

using var server = ServiceSelector.SelectService(out string serviceName);
CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, cancelEventArgs) => {
    cancelEventArgs.Cancel = false;
    cancellationTokenSource.Cancel();
};

var table = new Table().Title($"[bold]{serviceName} Service[/]").AddColumns("Endpoint", "Time", "Message").Expand().HeavyHeadBorder();
server.ServerEvent += (sender, message) => {
    if (sender is not IServer<IClient> server) return;
    AnsiConsole.Clear();
    table.AddRow(message.EndPoint?.ToString() ?? string.Empty, message.TimeStamp.ToString("H:mm:ss.ffff"), message.Message.TrimEnd());
    AnsiConsole.Write(table);
};

try {
    await AnsiConsole.Status().Spinner(Spinner.Known.Dots).StartAsync("Listening...", async context => {
        await server.Start(cancellationTokenSource.Token);
    });
} catch (Exception exception) {
    table.AddRow(string.Empty, DateTime.Now.ToString("H:mm:ss.ffff"), $"Exception: {exception.Message}");
}
AnsiConsole.WriteLine("Quitting");
