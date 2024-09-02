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
using ProtoHackersDotNet.GUI.MainView.Grader;
using ProtoHackersDotNet.GUI.MainView.Server;
using ProtoHackersDotNet.GUI.Serialization;
using ProtoHackersDotNet.Servers.UdpDatabase;
using ProtoHackersDotNet.Servers.MobProxy;
using ProtoHackersDotNet.Servers;

namespace ProtoHackersDotNet.GUI;

public class App : Application
{
    StateSerializer? stateSerializer;

    public const string AppName = "ProtoHackersDotNet";
    public static readonly Version Version = new(1, 1);
    public static readonly ProductInfoHeaderValue UserAgent = ProductInfoHeaderValue.Parse($"{AppName}/{Version}");

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Name = AppName;

        var serviceCollection = new ServiceCollection();
        var serviceProvider = ConfigureServices(serviceCollection).BuildServiceProvider();
        this.stateSerializer = serviceProvider.GetRequiredService<StateSerializer>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.Exit += (s, e) => stateSerializer.Save();
            desktop.MainWindow = serviceProvider.GetRequiredService<MainWindow>();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
            var mainVM = serviceProvider.GetRequiredService<MainViewModel>();
            singleViewPlatform.MainView = new MainView.MainView { DataContext = mainVM };
        }

        base.OnFrameworkInitializationCompleted();
    }

    static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // appsetting configuration options
        var config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                               .AddJsonFile("appsettings.json", optional: false)
                                               .AddDependentJsonFile("StateSerializationOptions:SavePath").Build();
        services.AddSingleton<IConfiguration>(config).EndChain();

        services.RegisterOption<GraderClientOptions>()
                .RegisterOption<ServerManagerState>()
                .RegisterOption<StartServerCommandState>()
                .RegisterOption<TestServerCommandState>()
                .RegisterOption<ClientVMFactoryOptions>()
                .RegisterOption<MessageManagerOptions>()
                .RegisterOption<BudgetChatServerOptions>()
                .RegisterOption<UdpDatabaseServerOptions>()
                .RegisterOption<MobProxyServerOptions>()
                .RegisterOption<StateSerializationOptions>()
                .AddSingleton<AppState>()
                .AddSingleton<StateSerializer>()
                .EndChain();

        // http options.
        services.AddHttpClient<GraderClient>(GraderClient.Configure).EndChain();

                // servers
        services.AddSingleton(Problem.Instances.All)
                .AddSingleton<IServer, EchoServer>()
                .AddSingleton<IServer, JsonPrimeServer>()
                .AddSingleton<IServer, PriceTrackerServer>()
                .AddSingleton<IServer, BudgetChatServer>()
                .AddSingleton<IServer, UdpDatabaseServer>()
                .AddSingleton<IServer, MobProxyServer>()
                // view elements
                .AddSingleton<MainWindow>()
                // VM elements
                .AddSingleton<GradingService>()
                .AddSingleton<MainViewModel>()
                .AddSingleton<ClientManager>()
                .AddSingleton<ClientVMFactory>()
                .AddSingleton<ServerManager>().AddResolver<ServerManager, IStateProvider>()
                .AddSingleton<MessageManager>()
                .AddSingleton<StartServerCommand>().AddResolver<StartServerCommand, IStateProvider>()
                .AddSingleton<ClearLogCommand>()
                .AddSingleton<TestServerCommand>().AddResolver<TestServerCommand, IStateProvider>()
                .EndChain();
        return services;
    }
}