using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainWindow : Window
{
    readonly StateSaver stateSaver;

    public MainWindow(MainViewModel mainViewModel, StateSaver stateSaver)
    {
        Title = $"{App.AppName} - {App.Version}";
        InitializeComponent();

        MainViewModel = mainViewModel;
        DataContext = MainViewModel;

        this.stateSaver = stateSaver;

        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e) => this.stateSaver.Save();

    public MainViewModel MainViewModel { get; }
}