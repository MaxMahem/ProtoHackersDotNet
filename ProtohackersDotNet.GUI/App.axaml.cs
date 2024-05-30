using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoHackersDotNet.GUI.MainView;
using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.Servers.Echo;
using ProtoHackersDotNet.Servers.JsonPrime;
using ProtoHackersDotNet.Servers.PriceTracker;
using System.Net.Http.Headers;
using ProtoHackersDotNet.Servers.BudgetChat;
using ProtoHackersDotNet.GUI.MainView.Server;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI;

public partial class App : Application
{
    public static readonly Version Version = new(1, 1);
    public const string AppName = "ProtoHackersDotNet";

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
        // http options.
        var userAgent = ProductInfoHeaderValue.Parse($"{AppName}/{Version}");
        services.AddSingleton(userAgent)
                .AddHttpClient<ProtoHackerApiClient>(client => client.DefaultRequestHeaders.UserAgent.Add(userAgent));

        // configuration options
        var config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                               .AddJsonFile("appsettings.json", optional: false)
                                               .AddJsonFile(StateSaver.SETTINGS_PATH, optional: true).Build();

        services.AddOptions<ProtoHackerApiClientOptions>().Bind(config.GetSection(nameof(ProtoHackerApiClientOptions)));
        services.AddOptions<ServerManagerState>().Bind(config.GetSection(nameof(ServerManagerState)));
        services.AddOptions<ClientManagerOptions>().Bind(config.GetSection(nameof(ClientManagerOptions)));
        services.AddOptions<BudgetChatServerOptions>().Bind(config.GetSection(nameof(BudgetChatServerOptions)));
        services.AddSingleton<StateSaver>();

        services.AddSingleton(config);

        // servers
        services.AddSingleton<IServer<IClient>, EchoServer>()
                .AddSingleton<IServer<IClient>, JsonPrimeServer>()
                .AddSingleton<IServer<IClient>, PriceTrackerServer>()
                .AddSingleton<IServer<IClient>, BudgetChatServer>();

        // view elements
        services.AddSingleton<MainWindow>();

        // VM elements
        return services.AddSingleton<ProtoHackerApiManager>()
                       .AddSingleton<MainViewModel>()
                       .AddSingleton<ClientManager>()
                       .AddSingleton<ServerManager>()
                       .AddSingleton<MessageManager>()
                       .AddSingleton<StartServerCommand>()
                       .AddSingleton<ClearLogCommand>()
                       .AddSingleton<TestServerCommand>();
    }
}