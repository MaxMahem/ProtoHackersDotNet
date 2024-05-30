using Avalonia.Controls.Primitives;
using Avalonia.Input;
using ProtoHackersDotNet.GUI.MainView.Messages;

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

    void SourceLabel_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender != SourceLabel) ThrowInvalidOperationException();
        var vm = SourceLabel.DataContext as MessageManager ?? ThrowInvalidOperationException<MessageManager>();
        if (vm.SourceFilter.Entries.Count is 0) return;

        FlyoutBase.ShowAttachedFlyout(SourceLabel);
    }

    void MessageLabel_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender != MessageLabel) ThrowInvalidOperationException();
        var vm = MessageLabel.DataContext as MessageManager ?? ThrowInvalidOperationException<MessageManager>();
        if (vm.SourceFilter.Entries.Count is 0) return;

        FlyoutBase.ShowAttachedFlyout(MessageLabel);
    }
}
