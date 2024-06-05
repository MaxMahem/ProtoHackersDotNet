namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public class DataTransmissionEvent(ITransmission transmission, MessageSource type) : DataEvent(transmission)
{
    public override MessageSource SourceType { get; } = type;
    public override string Type => nameof(DataTransmissionEvent);

    public static DataTransmissionEvent FromClient(IClient client, ITransmission transmission)
        => new(transmission, MessageSource.Client) {
            Source = client.LocalEndPoint.ToString(),
        };

    public static DataTransmissionEvent FromServer(IServer server, ITransmission transmission)
        => new(transmission, MessageSource.Server) {
            Source = server.LocalEndPoint!.ToString(),
        };
}