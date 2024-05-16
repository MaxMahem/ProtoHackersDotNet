using ProtoHackersDotNet.Servers.Echo;
using ProtoHackersDotNet.Servers.Interface.Client;
using ProtoHackersDotNet.Servers.JsonPrime;
using ProtoHackersDotNet.Servers.PriceTracker;
using ProtoHackersDotNet.Servers.BudgetChat;

namespace ProtoHackersDotNet.Servers.Interface.Server;

public static class ServerFactories
{
    public static IServerFactory Echo { get; } = new ServerFactory<EchoServer>();
    public static IServerFactory JsonPrime { get; } = new ServerFactory<JsonPrimeServer>();
    public static IServerFactory PriceTracker { get; } = new ServerFactory<PriceTrackerServer>();
    public static IServerFactory BudgetChat { get; } = new ServerFactory<BudgetChatServer>();

    public static ImmutableArray<IServerFactory> Avaliable { get; }
        = [Echo, JsonPrime, PriceTracker, BudgetChat];
}
