using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ProtoHackersDotNet.Servers.Echo;
using ProtoHackersDotNet.Servers.JsonPrime;
using ProtoHackersDotNet.Servers.PriceTracker;
using ProtoHackersDotNet.Servers.BudgetChat;
using ProtoHackersDotNet.GUI.MainView;
using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.GUI.MainView.Server;
using ProtoHackersDotNet.GUI.Serialization;
using ProtoHackersDotNet.Servers.UdpDatabase;
using Microsoft.Extensions.Options;
using ProtoHackersDotNet.Servers.MobProxy;

namespace ProtoHackersDotNet.GUI;

public class App : Application
{
    public const string AppName = "ProtoHackersDotNet";
    public static readonly Version Version = new(1, 1);
    static readonly ProductInfoHeaderValue UserAgent = ProductInfoHeaderValue.Parse($"{AppName}/{Version}");

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Name = AppName;

        var serviceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = serviceProvider.GetService<MainWindow>();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
            var mainVM = serviceProvider.GetService<MainViewModel>();
            singleViewPlatform.MainView = new MainView.MainView { DataContext = mainVM };
        }

        base.OnFrameworkInitializationCompleted();
    }

    static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // configuration options
        var config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                               .AddJsonFile("appsettings.json", optional: false)
                                               .AddJsonFile(StateSaver.SETTINGS_PATH, optional: true).Build();
        services.AddSingleton<IConfiguration>(config).EndChain();

        services.Configure<ServerManagerState>(config.GetSection(nameof(ServerManagerState)));
        var protoHackerApiClientConfig = config.GetSection(nameof(ProtoHackerApiClientOptions));
        services.AddOptions<ProtoHackerApiClientOptions>().Bind(protoHackerApiClientConfig)
                .ValidateAndAddResolver()
                .RegisterOption<ServerManagerState>()
                .RegisterOption<ClientVMFactoryOptions>()
                .RegisterOption<MessageManagerOptions>()
                .RegisterOption<BudgetChatServerOptions>()
                .RegisterOption<UdpDatabaseServerOptions>()
                .RegisterOption<MobProxyServerOptions>()
                .AddSingleton<StateSaver>()
                .EndChain();

        // http options.
        services.AddHttpClient<ProtoHackerApiClient>(ConfigureClient);
        void ConfigureClient(HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.Add(UserAgent);
            client.BaseAddress = protoHackerApiClientConfig.GetValue<Uri>("BaseAddress");
        }

                // servers
        services.AddSingleton<IServer<IClient>, EchoServer>()
                .AddSingleton<IServer<IClient>, JsonPrimeServer>()
                .AddSingleton<IServer<IClient>, PriceTrackerServer>()
                .AddSingleton<IServer<IClient>, BudgetChatServer>()
                .AddSingleton<IServer<IClient>, UdpDatabaseServer>()
                .AddSingleton<IServer<IClient>, MobProxyServer>()
                // view elements
                .AddSingleton<MainWindow>()
                // VM elements
                .AddSingleton<ProtoHackerApiManager>()
                .AddSingleton<MainViewModel>()
                .AddSingleton<ClientManager>()
                .AddSingleton<ClientVMFactory>()
                .AddSingleton<ServerManager>().AddStateResolver<ServerManager>()
                .AddSingleton<MessageManager>()
                .AddSingleton<StartServerCommand>()
                .AddSingleton<ClearLogCommand>()
                .AddSingleton<TestServerCommand>()
                .EndChain();
        return services;
    }
}

public static class ServiceCollectionHelper
{
    public static IServiceCollection RegisterOption<T>(this IServiceCollection services) where T : class
        => services.AddOptions<T>().BindConfiguration(typeof(T).Name).ValidateAndAddResolver();
    public static IServiceCollection ValidateAndAddResolver<T>(this OptionsBuilder<T> services) where T : class
        => services.ValidateDataAnnotations().ValidateOnStart()
                   .Services
                   .AddSingleton(resolver => resolver.GetRequiredService<IOptions<T>>().Value);

    public static IServiceCollection AddStateResolver<T>(this IServiceCollection services)
        where T : class, IStateSaveable
        => services.AddSingleton<IStateSaveable>(provider => provider.GetRequiredService<ServerManager>());

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for discard")]
    public static void EndChain(this IServiceCollection services) { }
}