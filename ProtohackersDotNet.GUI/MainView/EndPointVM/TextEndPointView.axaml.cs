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

    public IObservableCommand<IPAddress>? GetIpCommand {
        get => this.getIpCommand;
        set => SetAndRaise(GetIpCommandProperty, ref this.getIpCommand, value);
    }
    IObservableCommand<IPAddress>? getIpCommand;

    public static readonly DirectProperty<TextEndPointView, TextEndPoint?> EndPointProperty =
        AvaloniaProperty.RegisterDirect<TextEndPointView, TextEndPoint?>(
            name: nameof(EndPoint),
            getter: endPointView => endPointView.EndPoint,
            setter: (endPointView, endPoint) => endPointView.EndPoint = endPoint);

    public TextEndPoint? EndPoint {
        get => this.endPoint;
        set => SetAndRaise(EndPointProperty, ref this.endPoint, value);
    }
    TextEndPoint? endPoint;

    public TextEndPointView()
    {
        InitializeComponent();

        IDisposable? unsubscribe = default;
        Loaded += (s, e) => unsubscribe = this.endPoint?.IP.Changes.Where(ip => ip is not null)
                                              .Subscribe(ip => this.EXTERNAL_IP.Text = ip?.ToString())
                                            ?? ThrowArgumentNullException<IDisposable>(nameof(EndPoint));
        Unloaded += (s, e) => unsubscribe?.Dispose();
    }
}