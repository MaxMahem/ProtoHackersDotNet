namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

/// <summary>Event marking the change in the server status, typically startup or shutdown.</summary>
/// <param name="server">The server that fired this event.</param>
/// <param name="type">The type of the event.</param>
/// <param name="message">The message to associate with this event.</param>
public class ServerStatusChangeEvent(IServer server, ServerEventType type, string message) : ServerEvent(server)
{
    public override ServerEventType EventType { get; } = ServerEventType.Start;
    public override string Type { get; } = $"Server{type}";

    public override MessageCategory Category => MessageCategory.StatusChange;

    public override string Message { get; } = message;

    public static ServerStatusChangeEvent Startup(IServer server) 
        => new(server, ServerEventType.Start, $"{server.Name} started and listening on {server.LocalEndPoint}");

    public static ServerStatusChangeEvent Shutdown(IServer server)
        => new(server, ServerEventType.Stop, $"{server.Name} stopped");

    public static ServerStatusChangeEvent Terminated<TException>(IServer server, TException exception) where TException : Exception
        => new(server, ServerEventType.Stop, $"{server.Name} terminated due to unhandled exception '{nameof(TException)}'");
}