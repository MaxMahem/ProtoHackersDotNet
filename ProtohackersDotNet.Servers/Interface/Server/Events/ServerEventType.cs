namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public enum ServerEventType
{
    Start,
    Stop,
    ClientConnect,
    ClientDisconnect,
    Broadcast,
    Exception,
}
