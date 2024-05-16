namespace ProtoHackersDotNet.Servers.Interface.Client;

public abstract class ClientMessage : EventArgs
{
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}

public class ClientEvent : ClientMessage, IDisplayMessage
{
    public required EndPoint? EndPoint { get; init; }
    public required ClientEventType EventType { get; init; }
    public string Type => EventType.ToString();
    public required string Message { get; init; }
}

public class NewClient : ClientMessage, IDisplayMessage
{
    public required IClient Client { get; init; }
    public EndPoint? EndPoint => Client.RemoteEndPoint;
    public string Type => "New Client";
    public string Message => $"New client from {EndPoint}.";
}

public class RemoteDisconnect : ClientMessage, IDisplayMessage
{
    public required IClient Client { get; init; }
    public EndPoint? EndPoint => Client.RemoteEndPoint;
    public required string Type { get; init; }
    public required string Message { get; init; }
}

public class DataTransmission : ClientMessage, IDisplayMessage
{
    public required EndPoint? EndPoint { get; init; }
    public string Type => Broadcast ? "Broadcast" : "Transmission";
    public required string Message { get; init; }
    public required bool Broadcast { get; init; }
    public required ByteSize BytesTransmitted { get; init; }
}

public class DataReciept : ClientMessage, IDisplayMessage
{
    public required EndPoint? EndPoint { get; init; }
    public string Type => "DataReciept";
    public required string Message { get; init; }
    public required ByteSize BytesRecieved { get; init; }
}