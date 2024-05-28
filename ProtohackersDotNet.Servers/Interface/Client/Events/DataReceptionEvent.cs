namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public class DataReceptionEvent(IClient client, ITransmission transmission) : ClientDataEvent(client, transmission)
{
    public override MessageSource MessageSource => MessageSource.RemoteClient;
    public override ClientEventType ClientEventType => ClientEventType.DataReceived;
    public override string Source { get; } = client.ClientEndPoint.ToString();
    public override string? Destination { get; } = client.Server.LocalEndPoint?.ToString()
        ?? ThrowHelper.ThrowInvalidOperationException<string>();
    public override string Type => nameof(DataReceptionEvent);
    public override string Message { get; } = string.IsNullOrEmpty(transmission.Translation) 
        ? $"{transmission.Data.ToByteSize()} received" : transmission.Translation.Trim();
}
