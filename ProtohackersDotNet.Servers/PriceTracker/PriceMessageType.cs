namespace ProtoHackersDotNet.Servers.PriceTracker;

public enum PriceMessageType : byte
{
    Insert = (byte)'I',
    Query  = (byte)'Q',
}