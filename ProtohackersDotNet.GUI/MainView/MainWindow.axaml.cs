using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel, StateSaver stateSaver)
    {
        Title = $"{App.AppName} - {App.Version}";
        InitializeComponent();

        MainViewModel = mainViewModel;
        DataContext = MainViewModel;

        Closing += (_, _) => stateSaver.Save();
    }

    public MainViewModel MainViewModel { get; }
}