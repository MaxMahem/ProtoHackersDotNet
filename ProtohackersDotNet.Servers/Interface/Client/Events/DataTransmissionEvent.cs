namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public class DataTransmissionEvent(IClient client, ITransmission transmission) : ClientDataEvent(client, transmission)
{
    public override MessageSource MessageSource => MessageSource.RemoteClient;
    public override ClientEventType ClientEventType => ClientEventType.DataTransmitted;
    public override string Type => nameof(DataTransmissionEvent);
    public override string Message { get; } = string.IsNullOrEmpty(transmission.Translation) 
        ? $"{transmission.Data.ToByteSize()} transmitted" : transmission.Translation.Trim();

    public override string Source { get; } = client.Server.LocalEndPoint?.ToString() 
        ?? ThrowHelper.ThrowInvalidOperationException<string>();
    public override string? Destination { get; } = client.ClientEndPoint.ToString();

    public static DataTransmissionEvent FromTransmission(IClient client, ITransmission transmission)
        => new(client, transmission);
}