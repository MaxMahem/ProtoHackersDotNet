namespace ProtoHackersDotNet.Servers.Interface.Client;

public enum DisplayMessageType
{
    Unclassified        = 0,
    ClientConnect       = 1 << 0, 
    ClientDisconnect    = 1 << 1, 
    ClientTransmission  = 1 << 2,
    ClientBroadcast     = 1 << 3,
    ClientReception     = 1 << 4,
    ClientException     = 1 << 5,
    TestRequest         = 1 << 6,
    TestRequestResponse = 1 << 7,
    TestLog             = 1 << 8, 
    TestSuccess         = 1 << 9,
    TestFailure         = 1 << 10,
    TestException       = 1 << 11,
    Exception           = 1 << 12,
}