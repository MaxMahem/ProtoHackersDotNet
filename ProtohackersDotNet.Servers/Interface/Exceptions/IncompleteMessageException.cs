namespace ProtoHackersDotNet.Servers.Interface.Exceptions;

public class IncompleteMessageException() : ClientException(new InvalidDataException("Incomplete client message"))
{
    [DoesNotReturn]
    public static void Throw(IClient client) => throw new IncompleteMessageException() { Client = client };
}