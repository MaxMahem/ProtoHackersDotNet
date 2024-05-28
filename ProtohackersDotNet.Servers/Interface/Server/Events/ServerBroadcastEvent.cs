namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ServerBroadcastEvent(IServer server, ITransmission transmission) : ServerEvent(server)
{
    public override ServerEventType EventType => ServerEventType.Broadcast;
    public override string Type => nameof(ServerBroadcastEvent);
    public override MessageCategory Category => MessageCategory.Data;
    public override string Message { get; } = string.IsNullOrEmpty(transmission.Translation)
        ? $"{transmission.Data.ToByteSize()} broadcast" : transmission.Translation.Trim();
}