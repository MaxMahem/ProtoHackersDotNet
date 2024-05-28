namespace ProtoHackersDotNet.Servers.Interface.Exceptions;

public class ClientException(Exception exception) : Exception(exception.Message, exception)
{
    public required IClient Client { get; init; }

    [DoesNotReturn]
    public static void Throw(Exception exception, IClient client) 
        => throw new ClientException(exception) { Client = client };
}