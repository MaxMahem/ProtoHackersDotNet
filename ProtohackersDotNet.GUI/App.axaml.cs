using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ProtoHackersDotNet.GUI.MainView;

namespace ProtoHackersDotNet.GUI;

public partial class App : Application
{
    public static readonly Version Version = new(1, 0);
    public const string AppName = "ProtoHackersDotNet";

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Name = AppName;

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        var settings = MainViewModelSettings.LoadSettings();
        var mainVM = new MainViewModel(settings);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow { DataContext = mainVM };
            desktop.Exit += mainVM.OnExit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
            singleViewPlatform.MainView = new MainView.MainView { DataContext = mainVM };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
