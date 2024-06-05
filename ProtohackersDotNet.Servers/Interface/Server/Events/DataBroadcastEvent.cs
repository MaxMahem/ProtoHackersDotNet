namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class DataBroadcastEvent(ITransmission transmission) : DataEvent(transmission)
{
    public override string Type => nameof(DataBroadcastEvent);
    public override MessageSource SourceType => MessageSource.Server;

    public static DataBroadcastEvent FromServer(ITransmission transmission, IServer server)
        => new(transmission) {
            Source = server.LocalEndPoint!.ToString(),
        };
}