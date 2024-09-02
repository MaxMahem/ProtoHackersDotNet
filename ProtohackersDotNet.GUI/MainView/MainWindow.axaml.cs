using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        Title = $"{App.AppName} - {App.Version}";
        InitializeComponent();

        MainViewModel = mainViewModel;
        DataContext = MainViewModel;
    }

    public MainViewModel MainViewModel { get; }
}