namespace ProtoHackersDotNet.Servers.Interface;

public interface IDisplayMessage
{
    EndPoint? EndPoint { get; }
    DateTime TimeStamp { get; }
    string Type { get; }
    string Message { get; }
}
