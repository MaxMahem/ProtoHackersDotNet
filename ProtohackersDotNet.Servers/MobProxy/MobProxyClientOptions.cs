namespace ProtoHackersDotNet.Servers.MobProxy;

public sealed class MobProxyClientOptions(MobProxyServerOptions options)
{
    public ReadOnlyMemory<byte> ReplacementAddress { get; } = Encoding.ASCII.GetBytes(options.ReplacementAddress);
    public UrlEndPoint ChatServer { get; } = options.ChatServer;
}