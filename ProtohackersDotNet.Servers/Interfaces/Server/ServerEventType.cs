namespace ProtoHackersDotNet.Servers.Interfaces.Server;

public enum ServerEventType
{
    Start,
    Stop,
    ClientConnect,
    ClientDisconnect,
    Broadcast,
    Exception,
}
