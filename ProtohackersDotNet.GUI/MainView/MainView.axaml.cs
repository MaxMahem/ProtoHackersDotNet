using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        // Unloaded += MainView_Unloaded;
    }

    private void MainView_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        
    }

    void ComboBox_DropDownOpened(object? sender, EventArgs e) => (DataContext as MainViewModel)?.RefreshLocalIPs();

    private void SourceLabel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender != SourceLabel) return;
        FlyoutBase.ShowAttachedFlyout(SourceLabel);
    }
}
