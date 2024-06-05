namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public class DataReceptionEvent(string translation) : DataEvent(translation)
{
    public override MessageSource SourceType => MessageSource.Client;
    public override string Type => nameof(DataTransmissionEvent);

    public static DataReceptionEvent FromClient(IClient client, string translation)
            => new(translation) {
                Source = client.ClientEndPoint.ToString(),
            };

    public static DataReceptionEvent FromServer(IServer server, string translation, IPEndPoint source)
        => new(translation) {
            Source = source.ToString(),
        };
}