using Avalonia.Interactivity;
using ProtoHackersDotNet.GUI.Helpers;

namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public partial class TextEndPointView : UserControl
{
    public static readonly DirectProperty<TextEndPointView, IObservableCommand<IPAddress>?> GetIpCommandProperty =
        AvaloniaProperty.RegisterDirect<TextEndPointView, IObservableCommand<IPAddress>?>(
            name: nameof(GetIpCommand),
            getter: endPointView => endPointView.GetIpCommand,
            setter: (endPointView, ipLookupCommand) => endPointView.GetIpCommand = ipLookupCommand);

    IObservableCommand<IPAddress>? getIpCommand;

    public IObservableCommand<IPAddress>? GetIpCommand {
        get => this.getIpCommand;
        set => SetAndRaise(GetIpCommandProperty, ref this.getIpCommand, value);
    }

    public static readonly DirectProperty<TextEndPointView, TextEndPoint?> EndPointProperty =
        AvaloniaProperty.RegisterDirect<TextEndPointView, TextEndPoint?>(
            name: nameof(EndPoint),
            getter: endPointView => endPointView.EndPoint,
            setter: (endPointView, endPoint) => endPointView.EndPoint = endPoint);

    TextEndPoint? endPoint;

    public TextEndPoint? EndPoint {
        get => this.endPoint;
        set => SetAndRaise(EndPointProperty, ref this.endPoint, value);
    }

    public TextEndPointView()
    {
        InitializeComponent();
        Loaded += TextEndPointView_Loaded;
    }

    void TextEndPointView_Loaded(object? s, RoutedEventArgs e)
    {
        if (EndPoint is null) { ThrowInvalidOperationException(); }
        EndPoint.IP.Changes.Where(ip => ip is not null).Subscribe(ip => REMOTE_IP.Text = ip?.ToString()).DiscardUnsubscribe();
    }
}