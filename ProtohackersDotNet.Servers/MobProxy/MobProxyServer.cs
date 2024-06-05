namespace ProtoHackersDotNet.Servers.MobProxy;

public class MobProxyServer(MobProxyServerOptions options) : TcpServerBase<MobProxyClient>
{
    public ReadOnlyMemory<byte> ReplacementAddress = Encoding.ASCII.GetBytes(options.ReplacementAddress);

    public override ServerName Name { get; } =  ServerName.From(nameof(MobProxyServer));

    public override Problem Solution { get; } = new(5, "MobProxy");

    readonly MobProxyClientOptions clientOptions = new(options);

    protected override MobProxyClient CreateClient(TcpClient client, CancellationToken token) => new(clientOptions, client, token);
}