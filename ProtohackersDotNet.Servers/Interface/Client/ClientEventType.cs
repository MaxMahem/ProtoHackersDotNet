namespace ProtoHackersDotNet.Servers.Interface.Client;

public enum ClientEventType
{
    DataReceived,
    DataTransmitted,
    LineTranslation,
    Exception,
    Other,
}
