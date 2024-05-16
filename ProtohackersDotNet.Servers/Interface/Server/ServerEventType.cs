namespace ProtoHackersDotNet.Servers.Interface.Server;

public enum ServerEventType
{
    Start,
    Stop,
    ClientConnect,
    ClientDisconnect,
    Broadcast,
    Exception,
}
