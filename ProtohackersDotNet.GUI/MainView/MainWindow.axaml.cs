namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Title = $"{App.AppName} - {App.Version}";
        InitializeComponent();
    }
}
