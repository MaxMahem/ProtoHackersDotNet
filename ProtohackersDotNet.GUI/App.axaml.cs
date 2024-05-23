using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProtoHackersDotNet.GUI.MainView;
using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.ApiTest;
using ProtoHackersDotNet.Servers.Echo;
using ProtoHackersDotNet.Servers.JsonPrime;
using ProtoHackersDotNet.Servers.PriceTracker;
using System.Net.Http.Headers;
using ProtoHackersDotNet.Servers.BudgetChat;

namespace ProtoHackersDotNet.GUI;

public partial class App : Application
{
    public static readonly Version Version = new(1, 1);
    public const string AppName = "ProtoHackersDotNet";

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Name = AppName;

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var serviceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();
        var mainVM = serviceProvider.GetService<MainViewModel>() ?? ThrowHelper.ThrowArgumentNullException<MainViewModel>();

        // var thing = serviceProvider.GetService<IOptions<ProblemTestingManagerOptions>>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow { DataContext = mainVM };
            desktop.Exit += mainVM.OnExit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
            singleViewPlatform.MainView = new MainView.MainView { DataContext = mainVM };
        }

        base.OnFrameworkInitializationCompleted();
    }

    static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var userAgent = ProductInfoHeaderValue.Parse($"{AppName}/{Version}");
        services.AddSingleton(userAgent)
                .AddHttpClient<ApiTestManager>(client => client.DefaultRequestHeaders.UserAgent.Add(userAgent));

        var config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                               .AddJsonFile("appsettings.json", true)
                                               .AddJsonFile(MainViewModelOptions.SETTINGS_PATH, true).Build();

        services.AddOptions<ApiTestManagerOptions>().Bind(config.GetSection(nameof(ApiTestManagerOptions)));
        services.AddOptions<MainViewModelOptions>().Bind(config.GetSection(nameof(MainViewModelOptions)));
        services.AddOptions<ClientManagerOptions>().Bind(config.GetSection(nameof(ClientManagerOptions)));
        services.AddOptions<BudgetChatServerOptions>().Bind(config.GetSection(nameof(BudgetChatServerOptions)));

        services.AddSingleton<IServer<IClient>, EchoServer>()
                .AddSingleton<IServer<IClient>, JsonPrimeServer>()
                .AddSingleton<IServer<IClient>, PriceTrackerServer>()
                .AddSingleton<IServer<IClient>, BudgetChatServer>();
        
        return services.AddSingleton(config)
                       .AddSingleton<ApiTestManager>()
                       .AddSingleton<MainViewModel>()
                       .AddSingleton<ClientManager>();
    }
}
