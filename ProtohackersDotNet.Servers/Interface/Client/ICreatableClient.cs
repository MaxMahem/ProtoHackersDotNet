namespace ProtoHackersDotNet.Servers.Interface.Client;

public interface ICreatableClient<out TSelf> : IClient
    where TSelf : ICreatableClient<TSelf>
{
    abstract static TSelf Create(TcpClient client, CancellationToken token);
}