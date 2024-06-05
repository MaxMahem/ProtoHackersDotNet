namespace ProtoHackersDotNet.Servers.Interface;

public class UrlEndPoint
{
    public required string Host { get; init; }
    public required ushort Port { get; init; }
}