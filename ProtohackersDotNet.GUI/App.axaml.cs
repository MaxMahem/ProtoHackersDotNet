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
using Microsoft.Extensions.Options;
using ProtoHackersDotNet.Servers.MobProxy;
using ProtoHackersDotNet.Servers;

namespace ProtoHackersDotNet.GUI;

public class App : Application
{
    public const string AppName = "ProtoHackersDotNet";
    public static readonly Version Version = new(1, 1);
    public static readonly ProductInfoHeaderValue UserAgent = ProductInfoHeaderValue.Parse($"{AppName}/{Version}");

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

        services.RegisterOption<GraderClientOptions>()
                .RegisterOption<ServerManagerState>()
                .RegisterOption<StartServerCommandState>()
                .RegisterOption<TestServerCommandState>()
                .RegisterOption<ClientVMFactoryOptions>()
                .RegisterOption<MessageManagerOptions>()
                .RegisterOption<BudgetChatServerOptions>()
                .RegisterOption<UdpDatabaseServerOptions>()
                .RegisterOption<MobProxyServerOptions>()
                .AddSingleton<StateSaver>()
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
                .AddSingleton<ServerManager>().AddResolver<ServerManager, IStateSaveable>()
                .AddSingleton<MessageManager>()
                .AddSingleton<StartServerCommand>().AddResolver<StartServerCommand, IStateSaveable>()
                .AddSingleton<ClearLogCommand>()
                .AddSingleton<TestServerCommand>().AddResolver<TestServerCommand, IStateSaveable>()
                .EndChain();
        return services;
    }
}

public static class ServiceCollectionHelper
{
    public static IServiceCollection RegisterOption<T>(this IServiceCollection services) where T : class
        => services.AddOptions<T>().BindConfiguration(typeof(T).Name)
                   .ValidateDataAnnotations().ValidateOnStart()
                   .Services
                   .AddSingleton(provider => provider.GetRequiredService<IOptions<T>>().Value);

    public static IServiceCollection AddResolver<TType, TInterface>(this IServiceCollection services)
        where TType : class, TInterface
        where TInterface : class
        => services.AddSingleton<TInterface>(provider => provider.GetRequiredService<TType>());

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for discard")]
    public static void EndChain(this IServiceCollection services) { }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for discard")]
    public static void EndChain(this IHttpClientBuilder builder) { }
}